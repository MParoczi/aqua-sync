# Eheim Digital API Developer Guide for Electron React Applications

Building an Electron React application for Windows to control **Eheim Professional 5e filters** is fully supported through official REST and WebSocket APIs. Eheim provides comprehensive documentation at api.eheimdigital.com, and the devices use standard protocols over local WiFi networks without cloud dependencies.

## Official API documentation exists at api.eheimdigital.com

Eheim maintains an **official REST API portal** at https://api.eheimdigital.com/ with complete documentation, interactive testing tools, and code generation. The API supports all Professional 5e models (350, 450, 700, 600T) plus the entire Eheim Digital product line. Communication happens entirely on your local network using standard HTTP REST calls and WebSocket connections—no cloud services required for core functionality.

The Professional 5e series uses a **dual-protocol architecture**: REST API for configuration commands and WebSocket for real-time status updates. Both protocols work simultaneously, allowing your Electron app to POST control commands via REST while subscribing to live filter status via WebSocket. Authentication uses HTTP Basic Auth with default credentials api:admin (BASE64-encoded as YXBpOmFkbWlu), changeable via API or reset with the physical button.

## Device discovery through mDNS and master/client architecture

Eheim Digital devices implement a **master/client topology** where certain devices (Professional 5e filters, classicLEDcontrol+e, classicVARIO+e) act as master API servers, while others (heaters, feeders, pH controllers) connect as clients through a master. Discovery works through mDNS with the hostname **eheimdigital.local** or direct IP addresses obtained via DHCP.

For your Electron app, implement device discovery using Node.js libraries like **bonjour** or **mdns**. Search for services advertising `_http._tcp` and filter for "eheim" in the service name. The device obtains a local IP (e.g., 192.168.2.5) and responds to REST requests at `http://[device-ip]/api/[endpoint]`. Each device has a unique MAC address printed on its label, used for routing commands in multi-device setups.

### Discovery implementation pattern

```javascript
const Bonjour = require('bonjour');
const bonjour = Bonjour();

// Discover Eheim devices on network
bonjour.find({ type: 'http' }, (service) => {
  if (service.name.toLowerCase().includes('eheim')) {
    const deviceIP = service.addresses[0];
    console.log(`Found Eheim device at ${deviceIP}`);
    connectToDevice(deviceIP);
  }
});
```

For Professional 5e filters acting as masters, client devices communicate through them using a `to` parameter containing the target MAC address: `http://192.168.2.5/api/filter?to=A8:48:FA:D7:A0:F7`. The MESH_NETWORK response from the WebSocket connection reveals the complete device topology.

## Authentication requires HTTP Basic Auth with changeable credentials

Every API request requires an **Authorization header** with BASE64-encoded credentials. The factory default is username `api` and password `admin`, encoded as `YXBpOmFkbWlu`. Include this header in all HTTP requests:

```javascript
const headers = {
  'Authorization': 'Basic YXBpOmFkbWlu',
  'Content-Type': 'application/json'
};
```

Credentials can be changed via POST to `/api/changeauth` with a JSON body containing new username and password. To reset to factory defaults, hold the device's touch sensor for 5-8 seconds until the LED cycles through colors. This physical reset prevents lockouts if credentials are forgotten.

**Security considerations**: The protocol uses plain HTTP Basic Auth on local networks. For production apps, validate that communication happens only on private IP ranges (192.168.x.x, 10.x.x.x) and consider implementing TLS if the device supports HTTPS. The official documentation indicates minimum firmware version S2037 (master) / S1025 (client) for stable API operation.

## Power control through REST endpoints and WebSocket monitoring

Professional 5e filters support **on/off control** and continuous speed adjustment through multiple API methods. The primary approach uses the filter's operating mode combined with speed parameters:

### REST API power control

```javascript
// Turn filter on with manual mode at specific speed
const setPower = async (deviceIP, macAddress, isOn, speed) => {
  const url = `http://${deviceIP}/api/filter`;
  const body = {
    to: macAddress,
    filterActive: isOn ? 1 : 0,
    pumpMode: 1,  // 1=Manual, 2=Pulse, 3=Reserved, 4=Bio
    rotorSpeed: speed  // 0-15 scale
  };
  
  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Authorization': 'Basic YXBpOmFkbWlu',
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(body)
  });
  
  return response.ok;
};
```

POST requests follow a **fire-and-forget pattern**—they return HTTP 200 if authentication succeeds and JSON parses correctly, but don't confirm the device accepted the values. Always follow POST with GET to verify the filter stored the new settings.

### WebSocket status monitoring

The WebSocket protocol at `ws://[device-ip]/ws` provides real-time status updates. Connect using the **ws** library for Node.js:

```javascript
const WebSocket = require('ws');

const ws = new WebSocket('ws://192.168.2.5/ws');

ws.on('open', () => {
  // Request current filter data
  ws.send(JSON.stringify({
    title: 'GET_FILTER_DATA',
    from: 'USER',
    to: 'MASTER'
  }));
});

ws.on('message', (data) => {
  const message = JSON.parse(data);
  
  if (message.title === 'FILTER_DATA') {
    console.log('Filter active:', message.filterActive);
    console.log('Current frequency:', message.freq);
    console.log('Target frequency:', message.freqSoll);
    console.log('Rotation speed:', message.rotSpeed);
    console.log('Pump mode:', message.pumpMode);
  }
});
```

WebSocket messages arrive automatically when device state changes, typically every 15 seconds, plus immediately after commands execute. The `filterActive` field (0 or 1) indicates power state, while `freq` shows actual pump frequency in Hz.

## Constant flow mode maintains output despite filter clogging

Constant flow mode is one of four operating modes (Manual, Pulse, Bio, Constant). This mode uses **electronic pollution detection** to automatically increase pump frequency as the filter loads with debris, maintaining consistent water flow throughout the maintenance cycle.

### Technical operation

The Professional 5e monitors filter resistance through frequency feedback. As mechanical filtration accumulates waste, flow restriction increases. The controller compensates by raising frequency from minimum (~3500 Hz for 5e 350) toward maximum (~7100 Hz for 5e 350). When maximum frequency is reached without achieving target flow, the service indicator triggers.

### Configuration via REST API

```javascript
const setConstantFlow = async (deviceIP, macAddress, targetFlow) => {
  const url = `http://${deviceIP}/api/filter`;
  const body = {
    to: macAddress,
    pumpMode: 3,  // Constant flow mode (verify actual enum value)
    flowRate: targetFlow,  // Target flow rate in L/h
    filterActive: 1
  };
  
  await fetch(url, {
    method: 'POST',
    headers: {
      'Authorization': 'Basic YXBpOmFkbWlu',
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(body)
  });
};
```

**Important note**: Community reverse engineering reveals flow levels use **discrete 0-10 steps** internally, not continuous L/h values. The web interface maps these to model-specific flow rates:

- **5e 350**: 150-1500 L/h (max ~72 Hz)
- **5e 450**: 350-1700 L/h (max ~76 Hz)
- **5e 700/600T**: 400-1850 L/h (max ~80 Hz)

Your Electron app should present flow rates in L/h to users but internally convert to the 0-10 scale when sending commands. Monitor the `freq` and `freqSoll` fields via WebSocket to display real-time compensation happening as the filter clogs.

## Bio mode implements day/night flow cycles with time scheduling

Bio mode (also called "Sun/Moon mode") alternates between two flow rates on a time schedule, simulating natural current variations. This mode particularly benefits planted aquariums by reducing flow during "night" to allow CO2 accumulation while maintaining higher flow during "day" for nutrient distribution.

### Complete bio mode configuration

Bio mode requires **four parameters**: day flow level, night flow level, day start time, and night start time (all in minutes since midnight). Community reverse engineering provides the exact command structure:

```javascript
const setBioMode = async (deviceIP, macAddress, config) => {
  const url = `http://${deviceIP}/api/filter`;
  
  // Convert HH:MM to minutes since midnight
  const timeToMinutes = (timeStr) => {
    const [hours, minutes] = timeStr.split(':').map(Number);
    return hours * 60 + minutes;
  };
  
  const body = {
    title: 'START_NOCTURNAL_MODE',
    to: macAddress,
    from: 'USER',
    pumpMode: 4,  // Bio mode
    dfs_soll_day: config.dayFlowLevel,  // 0-10 scale
    dfs_soll_night: config.nightFlowLevel,  // 0-10 scale
    end_time_night_mode: timeToMinutes(config.dayStartTime),  // e.g. "08:00"
    start_time_night_mode: timeToMinutes(config.nightStartTime)  // e.g. "20:00"
  };
  
  await fetch(url, {
    method: 'POST',
    headers: {
      'Authorization': 'Basic YXBpOmFkbWlu',
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(body)
  });
};

// Example usage
await setBioMode('192.168.2.5', 'A8:48:FA:D7:A0:F7', {
  dayFlowLevel: 8,        // 80% flow during day
  nightFlowLevel: 3,      // 30% flow during night
  dayStartTime: '08:00',  // Day begins at 8:00 AM
  nightStartTime: '20:00' // Night begins at 8:00 PM
});
```

### Monitoring bio mode status

WebSocket FILTER_DATA messages reveal current bio mode state:

```javascript
{
  "title": "FILTER_DATA",
  "pumpMode": 4,                    // 4 indicates Bio mode active
  "nm_dfs_soll_day": 8,             // Day flow level (0-10)
  "nm_dfs_soll_night": 3,           // Night flow level (0-10)
  "end_time_night_mode": 480,       // Day starts: 480 min = 8:00 AM
  "start_time_night_mode": 1200,    // Night starts: 1200 min = 8:00 PM
  "freq": 5400,                     // Current frequency adjusts based on time
  "filterActive": 1
}
```

The device automatically transitions between day and night flow levels at the scheduled times. Your React UI can display the current mode (day/night) by comparing the current time against the schedule, and show which flow level is active.

### Multi-device coordination

Bio mode can synchronize with other Eheim Digital devices when paired through the web interface. For example, linking with thermocontrol+e allows temperature reduction during night mode, while linking with classicLEDcontrol+e coordinates lighting dimming with flow reduction. These pairings happen at the device level through the MESH_NETWORK topology, not requiring external coordination from your app.

## Electron React implementation architecture and libraries

Structure your Electron app with **clear separation between main process (Node.js) and renderer (React)**. The main process handles all WebSocket and REST API communication, while the renderer displays UI and sends commands via IPC.

### Recommended architecture

```
electron-eheim-control/
├── main/
│   ├── api/
│   │   ├── rest-client.js       # HTTP REST wrapper
│   │   ├── websocket-client.js  # WebSocket connection manager
│   │   └── device-discovery.js  # mDNS discovery
│   ├── models/
│   │   └── filter-device.js     # Device state management
│   └── main.js                  # Electron main process
└── renderer/
    ├── components/
    │   ├── FilterControl.jsx    # Power, mode, speed controls
    │   ├── BioModeConfig.jsx    # Day/night schedule editor
    │   └── DeviceDiscovery.jsx  # Device list and connection
    ├── App.jsx
    └── index.jsx
```

### Essential Node.js libraries

Install these dependencies for your main process:

```bash
npm install ws bonjour axios electron
```

- **ws**: WebSocket client for real-time device communication
- **bonjour**: mDNS/DNS-SD for automatic device discovery
- **axios**: HTTP client for REST API calls (includes auth helpers)
- **electron**: Framework itself

### IPC communication pattern

```javascript
// main/main.js - Main process
const { ipcMain } = require('electron');
const FilterAPI = require('./api/filter-api');

ipcMain.handle('filter:setPower', async (event, deviceIP, mac, isOn) => {
  return await FilterAPI.setPower(deviceIP, mac, isOn);
});

ipcMain.handle('filter:setBioMode', async (event, deviceIP, mac, config) => {
  return await FilterAPI.setBioMode(deviceIP, mac, config);
});

// renderer/components/FilterControl.jsx - React component
import { ipcRenderer } from 'electron';

const FilterControl = ({ device }) => {
  const handlePowerToggle = async () => {
    const result = await ipcRenderer.invoke(
      'filter:setPower',
      device.ip,
      device.mac,
      !device.isOn
    );
    // Update UI based on result
  };
  
  return (
    <button onClick={handlePowerToggle}>
      {device.isOn ? 'Turn Off' : 'Turn On'}
    </button>
  );
};
```

### WebSocket state management

Maintain a single WebSocket connection per device in the main process, broadcasting state updates to the renderer:

```javascript
// main/api/websocket-client.js
const WebSocket = require('ws');
const { BrowserWindow } = require('electron');

class FilterWebSocketClient {
  constructor(deviceIP) {
    this.ws = new WebSocket(`ws://${deviceIP}/ws`);
    this.setupHandlers();
  }
  
  setupHandlers() {
    this.ws.on('message', (data) => {
      const message = JSON.parse(data);
      
      // Broadcast to all renderer windows
      BrowserWindow.getAllWindows().forEach(window => {
        window.webContents.send('filter:status-update', message);
      });
    });
    
    this.ws.on('error', (error) => {
      console.error('WebSocket error:', error);
    });
  }
  
  sendCommand(command) {
    if (this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(command));
    }
  }
}
```

## Working code examples and community integrations

Several production-ready implementations provide reference code for your Electron app:

### Python API wrapper (autinerd/eheimdigital)

The most mature integration is at **https://github.com/autinerd/eheimdigital**, a Python library used by the official Home Assistant Core integration (as of version 2025.1). While written in Python, the repository's source code reveals the complete protocol implementation including authentication flows, error handling, and reconnection logic. Key insights from this library:

- Uses aiohttp for async HTTP requests
- Maintains persistent WebSocket connections with automatic reconnection
- Implements 15-second polling for devices that don't push updates
- Handles the master/client routing automatically
- Supports all Professional 5e models with model-specific flow rate mappings

Translate the Python patterns to JavaScript using **async/await** with **axios** and **ws** libraries. The repository's `hub.py` shows device enumeration logic, while `devices/filter.py` contains Professional 5e specific commands.

### Home Assistant official integration

Home Assistant's official integration provides configuration examples at **https://www.home-assistant.io/integrations/eheimdigital/**. The integration exposes these entity types relevant to your Electron app:

**For Professional 5e filters:**
- **Switch**: Power on/off control
- **Select**: Mode selection (Manual, Pulse, Bio)
- **Number**: Manual speed (0-100%), Day speed, Night speed
- **Time**: Day start time, Night start time
- **Sensor**: Current speed, service hours remaining, error codes

Study how Home Assistant maps the discrete 0-10 flow levels to percentage values for user-friendly display. The integration normalizes device-specific frequency ranges (3500-7100 Hz for 5e 350) to 0-100% scales in the UI.

### Reverse-engineered WebSocket protocol documentation

The Home Assistant Community forum thread at **https://community.home-assistant.io/t/eheim-aquarium-filter/477103** contains detailed reverse engineering work including complete JSON message structures. A community member captured the exact WebSocket messages sent by the official Eheim web interface, revealing undocumented fields like `dfsFaktor` (flow factor) and `runTime` (total operating hours).

This community documentation fills critical gaps in the official API docs, particularly around the WebSocket protocol which is more reliable for real-time control than the REST API.

## Protocol gaps, limitations, and workarounds discovered

Through community experience and testing, several important limitations emerged:

### Unreliable calculated metrics

The **pollution grade percentage** shown in the Eheim web UI is not based on actual sensors—it's a calculated estimate. Community testing found filters showing "5% pollution" after years of operation, indicating the algorithm is unreliable. Similarly, service hour predictions can be wildly inaccurate. **Trust only direct measurements**: frequency (freq), rotation speed (rotSpeed), and configured parameters. Don't display calculated pollution percentages in your UI.

### POST request verification requirement

REST API POST requests return HTTP 200 on successful authentication and JSON parsing, **not** on successful parameter acceptance. The device validates parameters against min/max ranges independently after the HTTP response. Always follow POST with GET to confirm the device accepted your values:

```javascript
const setAndVerify = async (deviceIP, mac, params) => {
  // Send configuration
  await fetch(`http://${deviceIP}/api/filter`, {
    method: 'POST',
    headers: { 'Authorization': 'Basic YXBpOmFkbWlu' },
    body: JSON.stringify({ to: mac, ...params })
  });
  
  // Verify it took effect
  await new Promise(resolve => setTimeout(resolve, 1000));  // Brief delay
  const response = await fetch(`http://${deviceIP}/api/filter?to=${mac}`, {
    headers: { 'Authorization': 'Basic YXBpOmFkbWlu' }
  });
  const status = await response.json();
  
  return status;  // Check if params match what you sent
};
```

### WebSocket more reliable than REST for reads

Community integrations uniformly prefer **WebSocket for reading state** and REST only for sending commands. The WebSocket connection provides automatic updates, lower latency, and eliminates polling overhead. Structure your app to maintain a persistent WebSocket connection and only use REST POST for configuration changes.

### Discrete flow levels vs continuous values

Internally, filters use **11 discrete flow levels (0-10)**, not continuous L/h values. The web UI maps these to model-specific flow rates for display. When implementing sliders in React, use discrete steps rather than continuous ranges to match the device's actual capabilities:

```jsx
<input 
  type="range" 
  min={0} 
  max={10} 
  step={1}
  value={flowLevel}
  onChange={(e) => setFlowLevel(parseInt(e.target.value))}
/>
<span>{flowLevelToLiterPerHour(flowLevel, model)} L/h</span>
```

### Firmware version requirements

The REST API requires **minimum firmware S2037** (master devices) and **S1025** (client devices). Earlier versions (S2036/S1024) have known bugs. Check firmware version via the USRDTA WebSocket message:

```json
{
  "title": "USRDTA",
  "revision": [2037, 1025],
  "latestAvailableRevision": [2037, 1025],
  "firmwareAvailable": 0
}
```

Display a warning in your Electron app if the connected device has outdated firmware, directing users to https://eheim.com/en_GB/support/downloads/ for updates.

## Additional resources and technical references

### Official documentation

- **REST API Documentation**: https://api.eheimdigital.com/ - Complete endpoint reference with interactive testing
- **Product Specifications**: https://eheim.com/en_GB/aquatics/technology/external-filters/professionel-5e/ - Technical specs for all models
- **Firmware Downloads**: https://eheim.com/en_GB/support/downloads/ - Software updates and manuals
- **Mobile Apps**: iOS (https://apps.apple.com/us/app/eheim-digital-connect/id6501949799) and Android (https://play.google.com/store/apps/details?id=com.eheim.digital.connect) - Official apps for protocol observation

### GitHub repositories

- **autinerd/eheimdigital**: https://github.com/autinerd/eheimdigital - Production Python library (reference implementation)
- **davidm-glitch/home-assistant-eheim-digital**: https://github.com/davidm-glitch/home-assistant-eheim-digital - Custom HA component with additional examples

### Community resources

- **Home Assistant Integration**: https://www.home-assistant.io/integrations/eheimdigital/ - Official integration documentation
- **Reverse Engineering Thread**: https://community.home-assistant.io/t/eheim-aquarium-filter/477103 - Detailed WebSocket protocol analysis
- **pH Monitoring Project**: https://blog.derfredy.com/eheim-ph/ - WebSocket data logging example with bash scripts

### Protocol specifications

Both REST and WebSocket protocols operate on **local networks only** with no cloud dependency. The device creates either an access point (default) or joins your existing WiFi. All communication uses standard HTTP/WebSocket over port 80 with no encryption by default. The Professional 5e supports 802.11 b/g/n on 2.4 GHz bands.

The master/client architecture routes commands through master devices using MAC addresses as unique identifiers. Each device broadcasts its capabilities and topology via the MESH_NETWORK WebSocket message, allowing your app to build a complete device map automatically.

## Building your first Electron React Eheim control application

Start with this minimal implementation to establish communication:

```javascript
// main.js - Minimal Electron main process
const { app, BrowserWindow, ipcMain } = require('electron');
const WebSocket = require('ws');
const axios = require('axios');

let ws = null;
const AUTH_HEADER = 'Basic YXBpOmFkbWlu';

// Connect to filter WebSocket
ipcMain.handle('connect', async (event, deviceIP) => {
  ws = new WebSocket(`ws://${deviceIP}/ws`);
  
  ws.on('message', (data) => {
    const message = JSON.parse(data);
    event.sender.send('device-update', message);
  });
  
  return { success: true };
});

// Set filter power
ipcMain.handle('set-power', async (event, deviceIP, mac, isOn) => {
  await axios.post(`http://${deviceIP}/api/filter`, {
    to: mac,
    filterActive: isOn ? 1 : 0
  }, {
    headers: { 'Authorization': AUTH_HEADER }
  });
  
  return { success: true };
});

app.whenReady().then(() => {
  const win = new BrowserWindow({
    width: 1200,
    height: 800,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false
    }
  });
  
  win.loadFile('index.html');
});
```

```jsx
// App.jsx - Minimal React UI
import React, { useState, useEffect } from 'react';
import { ipcRenderer } from 'electron';

function App() {
  const [deviceIP, setDeviceIP] = useState('eheimdigital.local');
  const [connected, setConnected] = useState(false);
  const [filterData, setFilterData] = useState(null);
  
  useEffect(() => {
    ipcRenderer.on('device-update', (event, message) => {
      if (message.title === 'FILTER_DATA') {
        setFilterData(message);
      }
    });
  }, []);
  
  const handleConnect = async () => {
    const result = await ipcRenderer.invoke('connect', deviceIP);
    setConnected(result.success);
  };
  
  const handlePowerToggle = async () => {
    await ipcRenderer.invoke(
      'set-power',
      deviceIP,
      filterData.from,
      filterData.filterActive === 0
    );
  };
  
  return (
    <div>
      <h1>Eheim Filter Control</h1>
      
      {!connected ? (
        <div>
          <input 
            value={deviceIP} 
            onChange={(e) => setDeviceIP(e.target.value)}
            placeholder="Device IP or eheimdigital.local"
          />
          <button onClick={handleConnect}>Connect</button>
        </div>
      ) : (
        <div>
          <h2>Filter Status</h2>
          {filterData && (
            <>
              <p>Power: {filterData.filterActive ? 'ON' : 'OFF'}</p>
              <p>Frequency: {filterData.freq} Hz</p>
              <p>Mode: {['', 'Manual', 'Pulse', '', 'Bio'][filterData.pumpMode]}</p>
              <button onClick={handlePowerToggle}>Toggle Power</button>
            </>
          )}
        </div>
      )}
    </div>
  );
}

export default App;
```

This foundation establishes WebSocket communication and basic power control. Expand it by adding device discovery with the bonjour library, implementing bio mode configuration forms, and creating visualizations for frequency trends over time. The official API documentation at api.eheimdigital.com provides complete endpoint references for additional features like service counter reset, LED brightness control, and firmware updates.