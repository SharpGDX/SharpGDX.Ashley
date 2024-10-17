/*******************************************************************************
 * Copyright 2014 See AUTHORS file.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ******************************************************************************/

namespace SharpGDX.Ashley.Tests;

using SharpGDX.Desktop;
using SharpGDX.Graphics;
using SharpGDX.Ashley.Tests.Systems;
using SharpGDX.Ashley.Tests.Components;
using SharpGDX.Graphics.G2D;
using SharpGDX.Mathematics;

public class RenderSystemTest {
	public RenderSystemTest() {
		DesktopApplicationConfiguration config = new DesktopApplicationConfiguration();
		config.setWindowedMode(640, 480);

		new DesktopApplication(new MainClass(), config);
	}

	public class MainClass : ApplicationAdapter {
		PooledEngine engine;

		public override void Create () {
			OrthographicCamera camera = new OrthographicCamera(640, 480);
			camera.position.set(320, 240, 0);
			camera.update();

			Texture crateTexture = new Texture("assets/crate.png");
			Texture coinTexture = new Texture("assets/coin.png");

			engine = new PooledEngine();
			engine.addSystem(new RenderSystem(camera));
			engine.addSystem(new MovementSystem());

			Entity crate = engine.createEntity();
			crate.add(new PositionComponent(50, 50));
			crate.add(new VisualComponent(new TextureRegion(crateTexture)));

			engine.addEntity(crate);

			TextureRegion coinRegion = new TextureRegion(coinTexture);

			for (int i = 0; i < 100; i++) {
				Entity coin = engine.createEntity();
				coin.add(new PositionComponent(MathUtils.random(640), MathUtils.random(480)));
				coin.add(new MovementComponent(10.0f, 10.0f));
				coin.add(new VisualComponent(coinRegion));
				engine.addEntity(coin);
			}
		}

		public override void Render () {
			Gdx.gl.glClearColor(0, 0, 0, 1);
			Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

			engine.update(Gdx.graphics.getDeltaTime());
		}
	}
}
