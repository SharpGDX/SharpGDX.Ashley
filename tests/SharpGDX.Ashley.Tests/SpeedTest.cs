namespace SharpGDX.Ashley.Tests;

using SharpGDX.Utils;
using SharpGDX.Ashley.Tests.Components;
using Timer = SharpGDX.Ashley.Tests.Utils.Timer;
using SharpGDX.Ashley.Tests.Systems;

public class SpeedTest {
	public static int NUMBER_ENTITIES = 100000;

	public SpeedTest() {
		Timer timer = new Timer();
		Array<Entity> entities = new Array<Entity>();

		PooledEngine engine = new PooledEngine();

		engine.addSystem(new MovementSystem());

		Console.WriteLine("Number of entities: " + NUMBER_ENTITIES);

		/** Adding entities */
		timer.start("entities");

		entities.ensureCapacity(NUMBER_ENTITIES);

		for (int i = 0; i < NUMBER_ENTITIES; i++) {
			Entity entity = engine.createEntity();

			entity.add(new MovementComponent(10, 10));
			entity.add(new PositionComponent(0, 0));

			engine.addEntity(entity);

			entities.add(entity);
		}

		Console.WriteLine("Entities added time: " + timer.stop("entities") + "ms");

		/** Removing components */
		timer.start("componentRemoved");

		foreach (Entity e in entities) {
			// TODO: Why do we need type here? -RP
			// TODO: Look into changing remove to have another overload for when the return type should be the same as the passed in type. -RP
			e.remove<PositionComponent>(typeof(PositionComponent));
		}

		Console.WriteLine("Component removed time: " + timer.stop("componentRemoved") + "ms");

		/** Adding components */
		timer.start("componentAdded");

		foreach (Entity e in entities) {
			e.add(new PositionComponent(0, 0));
		}

		Console.WriteLine("Component added time: " + timer.stop("componentAdded") + "ms");

		/** System processing */
		timer.start("systemProcessing");

		engine.update(0);

		Console.WriteLine("System processing times " + timer.stop("systemProcessing") + "ms");

		/** Removing entities */
		timer.start("entitiesRemoved");

		engine.removeAllEntities();

		Console.WriteLine("Entity removed time: " + timer.stop("entitiesRemoved") + "ms");
	}
}
