namespace SharpGDX.Ashley.Tests.Components;

public class MovementComponent : Component {
	public float velocityX;
	public float velocityY;

	public MovementComponent (float velocityX, float velocityY) {
		this.velocityX = velocityX;
		this.velocityY = velocityY;
	}
}
