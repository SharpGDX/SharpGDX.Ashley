using SharpGDX.Ashley.Utils;
using SharpGDX.Ashley.Tests.Components;

namespace SharpGDX.Ashley.Tests;

public class BasicTest {

	public BasicTest() {
		PooledEngine engine = new PooledEngine();

		MovementSystem movementSystem = new MovementSystem();
		PositionSystem positionSystem = new PositionSystem();

		engine.addSystem(movementSystem);
		engine.addSystem(positionSystem);

		Listener listener = new Listener();
		engine.addEntityListener(listener);

		for (int i = 0; i < 10; i++) {
			Entity entity = engine.createEntity();
			entity.add(new PositionComponent(10, 0));
			if (i > 5) entity.add(new MovementComponent(10, 2));

			engine.addEntity(entity);
		}

		log("MovementSystem has: " + movementSystem.entities.size() + " entities.");
		log("PositionSystem has: " + positionSystem.entities.size() + " entities.");

		for (int i = 0; i < 10; i++) {
			engine.update(0.25f);

			if (i > 5) engine.removeSystem(movementSystem);
		}

		engine.removeEntityListener(listener);
	}

	public class PositionSystem : EntitySystem {
		public ImmutableArray<Entity> entities;

		public override void addedToEngine (Engine engine) {
			entities = engine.getEntitiesFor(Family.all(typeof(PositionComponent)).get());
			log("PositionSystem added to engine.");
		}

		public override void removedFromEngine (Engine engine) {
			log("PositionSystem removed from engine.");
			entities = null;
		}
	}

	public class MovementSystem : EntitySystem {
		public ImmutableArray<Entity> entities;

		private ComponentMapper<PositionComponent> pm = ComponentMapper<PositionComponent>.getFor<PositionComponent>(typeof(PositionComponent));
		private ComponentMapper<MovementComponent> mm = ComponentMapper<MovementComponent>.getFor<MovementComponent>(typeof(MovementComponent));

		public override void addedToEngine (Engine engine) {
			entities = engine.getEntitiesFor(Family.all(typeof(PositionComponent), typeof(MovementComponent)).get());
			log("MovementSystem added to engine.");
		}

        public override void removedFromEngine (Engine engine) {
			log("MovementSystem removed from engine.");
			entities = null;
		}

        public override void update (float deltaTime) {

			for (int i = 0; i < entities.size(); ++i) {
				Entity e = entities.get(i);

				PositionComponent p = pm.get(e);
				MovementComponent m = mm.get(e);

				p.x += m.velocityX * deltaTime;
				p.y += m.velocityY * deltaTime;
			}

			log(entities.size() + " Entities updated in MovementSystem.");
		}
	}

	public class Listener : EntityListener {

		public void entityAdded (Entity entity) {
			log("Entity added " + entity);
		}

		public void entityRemoved (Entity entity) {
			log("Entity removed " + entity);
		}
	}

	public static void log (String str) {
		Console.WriteLine(str);
	}
}
