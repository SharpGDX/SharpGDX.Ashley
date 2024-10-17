using SharpGDX.Graphics.G2D;

namespace SharpGDX.Ashley.Tests.Components;

public class VisualComponent : Component
{
    public TextureRegion region;

    public VisualComponent(TextureRegion region)
    {
        this.region = region;
    }
}