namespace SharpGDX.Ashley.Tests.Systems;

using SharpGDX.Ashley.Systems;
using SharpGDX.Ashley.Tests.Components;

public class MovementSystem : IteratingSystem {
	private ComponentMapper<PositionComponent> pm = ComponentMapper<PositionComponent>.getFor<PositionComponent>(typeof(PositionComponent));
	private ComponentMapper<MovementComponent> mm = ComponentMapper<MovementComponent>.getFor<MovementComponent>(typeof(MovementComponent));

	public MovementSystem () 
    : base(Family.all(typeof(PositionComponent), typeof(MovementComponent)).get())
    {
		
	}

	protected override void processEntity (Entity entity, float deltaTime) {
		PositionComponent position = pm.get(entity);
		MovementComponent movement = mm.get(entity);

		position.x += movement.velocityX * deltaTime;
		position.y += movement.velocityY * deltaTime;
	}
}
