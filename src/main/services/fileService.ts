/**
 * File Service for handling file operations
 *
 * This service manages file uploads, particularly for aquarium thumbnails.
 * Thumbnails are stored in the userData/thumbnails folder.
 */

import { app } from 'electron';
import * as fs from 'fs/promises';
import * as path from 'path';
import { randomUUID } from 'crypto';
import type { IpcResult } from '../../shared/types';

/**
 * Get the thumbnails directory path
 */
export function getThumbnailsPath(): string {
  return path.join(app.getPath('userData'), 'thumbnails');
}

/**
 * Ensure thumbnails directory exists
 */
async function ensureThumbnailsDirectory(): Promise<void> {
  const thumbnailsPath = getThumbnailsPath();
  try {
    await fs.access(thumbnailsPath);
  } catch {
    await fs.mkdir(thumbnailsPath, { recursive: true });
  }
}

/**
 * Copy a thumbnail file to the userData/thumbnails folder
 *
 * @param buffer - File buffer data
 * @param originalName - Original filename for extension extraction
 * @returns IpcResult with the relative path to the thumbnail
 */
export async function copyThumbnail(
  buffer: Buffer,
  originalName: string
): Promise<IpcResult<string>> {
  try {
    await ensureThumbnailsDirectory();

    // Get file extension
    const ext = path.extname(originalName);

    // Generate unique filename
    const filename = `${randomUUID()}${ext}`;

    // Full path to save the file
    const fullPath = path.join(getThumbnailsPath(), filename);

    // Write file
    await fs.writeFile(fullPath, buffer);

    // Return relative path (just the filename)
    return {
      success: true,
      data: filename,
    };
  } catch (err) {
    return {
      success: false,
      error: `Failed to copy thumbnail: ${(err as Error).message}`,
    };
  }
}

/**
 * Get the full path to a thumbnail file
 *
 * @param filename - The thumbnail filename
 * @returns IpcResult with the full path to the thumbnail
 */
export async function getThumbnailPath(filename: string): Promise<IpcResult<string>> {
  try {
    const fullPath = path.join(getThumbnailsPath(), filename);

    // Check if file exists
    try {
      await fs.access(fullPath);
    } catch {
      return {
        success: false,
        error: 'Thumbnail file not found',
      };
    }

    return {
      success: true,
      data: fullPath,
    };
  } catch (err) {
    return {
      success: false,
      error: `Failed to get thumbnail path: ${(err as Error).message}`,
    };
  }
}

/**
 * Delete a thumbnail file
 *
 * @param filename - The thumbnail filename to delete
 * @returns IpcResult indicating success or failure
 */
export async function deleteThumbnail(filename: string): Promise<IpcResult<boolean>> {
  try {
    const fullPath = path.join(getThumbnailsPath(), filename);

    // Try to delete the file
    try {
      await fs.unlink(fullPath);
    } catch (err) {
      // File might not exist, that's okay
      if ((err as NodeJS.ErrnoException).code !== 'ENOENT') {
        throw err;
      }
    }

    return {
      success: true,
      data: true,
    };
  } catch (err) {
    return {
      success: false,
      error: `Failed to delete thumbnail: ${(err as Error).message}`,
    };
  }
}
