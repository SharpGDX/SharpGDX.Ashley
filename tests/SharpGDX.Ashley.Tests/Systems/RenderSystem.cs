namespace SharpGDX.Ashley.Tests.Systems;

using SharpGDX.Ashley.Tests.Components;
using SharpGDX.Ashley.Utils;
using SharpGDX.Graphics.G2D;
using SharpGDX.Graphics;

public class RenderSystem : EntitySystem {
	private ImmutableArray<Entity> entities;

	private SpriteBatch batch;
	private OrthographicCamera camera;

	private ComponentMapper<PositionComponent> pm = ComponentMapper<PositionComponent>.getFor<PositionComponent>(typeof(PositionComponent));
	private ComponentMapper<VisualComponent> vm = ComponentMapper<VisualComponent>.getFor<VisualComponent>(typeof(VisualComponent));

	public RenderSystem (OrthographicCamera camera) {
		batch = new SpriteBatch();

		this.camera = camera;
	}

	public override void addedToEngine (Engine engine) {
		entities = engine.getEntitiesFor(Family.all(typeof(PositionComponent), typeof(VisualComponent)).get());
	}

	public override void removedFromEngine (Engine engine) {

	}

	public override void update (float deltaTime) {
		PositionComponent position;
		VisualComponent visual;

		camera.update();

		batch.begin();
		batch.setProjectionMatrix(camera.combined);

		for (int i = 0; i < entities.size(); ++i) {
			Entity e = entities.get(i);

			position = pm.get(e);
			visual = vm.get(e);

			batch.draw(visual.region, position.x, position.y);
		}

		batch.end();
	}
}
