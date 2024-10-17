namespace SharpGDX.Ashley.Tests;

public class IgnoreSystemTest {

	public IgnoreSystemTest() {
		PooledEngine engine = new PooledEngine();

		CounterSystem counter = new CounterSystem();
		IgnoredSystem ignored = new IgnoredSystem();

		engine.addSystem(counter);
		engine.addSystem(ignored);

		for (int i = 0; i < 10; i++) {
			engine.update(0.25f);
		}
	}

	private  class CounterSystem : EntitySystem {
        public override void update (float deltaTime) {
			log("Running " + GetType().Name);
		}
	}

	private  class IgnoredSystem : EntitySystem {

		int counter = 0;

        public override bool checkProcessing () {
			counter = 1 - counter;
			return counter == 1;
		}

		public override void update (float deltaTime) {
			log("Running " + GetType().Name);
		}
	}

	public static void log (String str) {
		Console.WriteLine(str);
	}
}
