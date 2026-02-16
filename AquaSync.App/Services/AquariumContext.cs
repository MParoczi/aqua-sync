using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;

namespace AquaSync.App.Services;

/// <summary>
/// Holds the currently selected aquarium for the active management shell session.
/// </summary>
public sealed class AquariumContext : IAquariumContext
{
    public Aquarium? CurrentAquarium { get; private set; }

    public bool IsReadOnly { get; private set; }

    public void SetCurrentAquarium(Aquarium aquarium)
    {
        CurrentAquarium = aquarium;
        IsReadOnly = aquarium.Status == AquariumStatus.Archived;
    }

    public void Clear()
    {
        CurrentAquarium = null;
        IsReadOnly = false;
    }
}
