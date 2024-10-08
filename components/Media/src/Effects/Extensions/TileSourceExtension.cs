// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Media.Pipelines;

namespace CommunityToolkit.WinUI.Media;

/// <summary>
/// An effect that loads an image and replicates it to cover all the available surface area
/// </summary>
/// <remarks>This effect maps to the Win2D <see cref="Graphics.Canvas.Effects.BorderEffect"/> effect</remarks>
public sealed partial class TileSourceExtension : ImageSourceBaseExtension
{
    /// <inheritdoc/>
    protected override object ProvideValue()
    {
        default(ArgumentNullException).ThrowIfNull(Uri);
        return PipelineBuilder.FromTiles(Uri, DpiMode, CacheMode);
    }
}
