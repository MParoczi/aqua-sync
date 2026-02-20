using AquaSync.App.Models;

namespace AquaSync.App.Contracts.Services;

/// <summary>
///     Holds the currently selected aquarium for the active management shell session.
///     Injected by child ViewModels to access the current aquarium's data.
/// </summary>
public interface IAquariumContext
{
    /// <summary>
    ///     The currently selected aquarium, or null if no aquarium is selected.
    /// </summary>
    Aquarium? CurrentAquarium { get; }

    /// <summary>
    ///     True when the current aquarium is archived (read-only mode).
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    ///     Sets the active aquarium context. Called by ShellPage when entering the management shell.
    ///     Automatically sets IsReadOnly based on the aquarium's status.
    /// </summary>
    void SetCurrentAquarium(Aquarium aquarium);

    /// <summary>
    ///     Clears the current aquarium context. Called when navigating back to the selector page.
    /// </summary>
    void Clear();
}
