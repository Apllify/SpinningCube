using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

using MonoGame.Extended;
using System.Diagnostics;
using System;

namespace SpinningCube
{
	public class SpinningCube : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		//cube information
		const float topLeftX = 250;
		const float topLeftY = 150;
		const float topLeftZ = 50;

		const float cubeSize = 100;
		
		Vector3[] frontSquare;
		Vector3[] backSquare;

		//rotation information
		Quaternion rotation;

		Vector3 rotAxisStart = new Vector3(0, 0, 0);
		Vector3 rotAxisEnd = new Vector3(topLeftX, topLeftY, 0);
		Vector3 rotAxis;
		Color axisColor = new Color(40, 40, 40, 60);

		float rotSpeed = 0.5f;
		const float rotIncrement = 0.3f;
		bool rotationActive = true;

		//input processing information
		KeyboardState lastKeyboard, curKeyboard;


		//UI
		SpriteFont mainFont;


		public SpinningCube()
		{
			_graphics = new GraphicsDeviceManager(this);
			_graphics.PreferredBackBufferWidth = 600;
			_graphics.PreferredBackBufferHeight = 400;
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			//initialize our points to their starting locations
			frontSquare = new Vector3[] {
				new(topLeftX, topLeftY, topLeftZ), 
				new(topLeftX + cubeSize, topLeftY, topLeftZ), 
				new(topLeftX + cubeSize, topLeftY + cubeSize, topLeftZ),
				new(topLeftX, topLeftY + cubeSize, topLeftZ)
			};

			backSquare = new Vector3[] { 
				new(topLeftX, topLeftY, topLeftZ - cubeSize),
				new(topLeftX + cubeSize, topLeftY, topLeftZ - cubeSize),
				new(topLeftX + cubeSize, topLeftY + cubeSize, topLeftZ - cubeSize),
				new(topLeftX, topLeftY + cubeSize, topLeftZ - cubeSize)
			};

			//instantiate our rotation
			rotAxis = rotAxisEnd - rotAxisStart;
			rotAxis.Normalize();
			rotation = Quaternion.CreateFromAxisAngle(rotAxis, rotSpeed);

			//make sure keyboard states aren't null
			lastKeyboard = Keyboard.GetState();
			curKeyboard = Keyboard.GetState();


			base.Initialize();
		}


		/// <summary>
		/// Short function that truncates a vector3 into a vector2
		/// </summary>
		/// <param name="v">The vector to be flattened</param>
		/// <returns>A flattened version of v</returns>
		private Vector2 Flat(Vector3 v)
		{
			return new(v.X, v.Y);
		}


		/// <summary>
		/// Function that converts a z-axis position into a usable 
		/// depth layer value for the spritebatch draw.
		/// </summary>
		private float LayerDepth(float z)
		{
			return 1 / (1 + MathF.Exp(z));
		}

		private float LayerDepth(ref Vector3 point)
		{
			return LayerDepth(point.Z);
		}

		/// <summary>
		/// Variant of layer depth that takes two points, 
		/// and returns the maximum of the two depths
		/// </summary>
		private float LayerDepth(ref Vector3 p1, ref Vector3 p2)
		{
			return MathF.Max(LayerDepth(ref p1), LayerDepth(ref p2));
		}

		private float LayerDepth( Vector3 p1,  Vector3 p2)
		{
			return MathF.Max(LayerDepth(ref p1), LayerDepth(ref p2));
		}

		private void DrawLine3D(SpriteBatch spriteBatch, Color color, ref Vector3 p1, ref Vector3 p2)
		{
			_spriteBatch.DrawLine(Flat(p1), Flat(p2),
					  color, 4,
					  LayerDepth(ref p1, ref p2));
		}

		private void DrawLine3D(SpriteBatch spriteBatch, Color color, ref Vector3 p1, Vector3 p2)
		{
			DrawLine3D(spriteBatch, color, ref p1, ref p2);
		}

		private void DrawLine3D(SpriteBatch spriteBatch, Color color, Vector3 p1, Vector3 p2)
		{
			DrawLine3D(spriteBatch, color, ref p1, ref p2);
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			mainFont = Content.Load < SpriteFont> ("MainFont");
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			curKeyboard = Keyboard.GetState();

			//disable rotation if prompted
			if (curKeyboard.IsKeyDown(Keys.Space) && !lastKeyboard.IsKeyDown(Keys.Space))
				rotationActive ^= true;

			//increase/decrease rotation speed
			if (curKeyboard.IsKeyDown(Keys.Right))
			{
				rotSpeed += rotIncrement * gameTime.GetElapsedSeconds();
			}
			else if (curKeyboard.IsKeyDown(Keys.Left))
			{
				rotSpeed = MathF.Max(rotSpeed - rotIncrement * gameTime.GetElapsedSeconds(), 0f);
			}

			//update rotation quaternion
			Quaternion.CreateFromAxisAngle(ref rotAxis, rotSpeed * gameTime.GetElapsedSeconds(), out rotation);

			//rotate all of our points (framerate dependent unfortunately)
			if (rotationActive)
			{
				for (int i = 0; i < frontSquare.Length; i++)
				{
					Vector3.Transform(ref frontSquare[i], ref rotation, out frontSquare[i]);
				}
				for (int i = 0; i < backSquare.Length; i++)
				{
					Vector3.Transform(ref backSquare[i], ref rotation, out backSquare[i]);
				}
			}

			lastKeyboard = curKeyboard;


			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.LightSkyBlue);
			_spriteBatch.Begin(sortMode: SpriteSortMode.BackToFront);



			//draw the background square
			int n = backSquare.Length; 
			for (int i = 0; i < n; i++)
			{
				DrawLine3D(_spriteBatch, Color.Blue, ref backSquare[i], ref backSquare[(i + 1) % n]);
			}
			DrawLine3D(_spriteBatch, Color.Blue, ref backSquare[0], ref backSquare[2]);
			DrawLine3D(_spriteBatch, Color.Blue, ref backSquare[1], ref backSquare[3]);


			//draw the square connections
			for (int i = 0; i<n; i++)
			{
				DrawLine3D(_spriteBatch, Color.Green, ref backSquare[i], ref frontSquare[i]);

				//DrawLine3D(_spriteBatch, Color.Green, ref frontSquare[i], ref backSquare[(i+1) % n]);
				//DrawLine3D(_spriteBatch, Color.Green, ref backSquare[i], ref frontSquare[(i+1) % n]);
			}



			// draw the foreground square
			int m = frontSquare.Length;
			for (int i = 0; i < m; i++)
			{
				DrawLine3D(_spriteBatch, Color.Red, ref frontSquare[i], ref frontSquare[(i + 1) % n]);
			}
			DrawLine3D(_spriteBatch, Color.Red, ref frontSquare[0], ref frontSquare[2]);
			DrawLine3D(_spriteBatch, Color.Red, ref frontSquare[1], ref frontSquare[3]);

			//draw the rotation axis
			DrawLine3D(_spriteBatch, axisColor, ref rotAxisStart, rotAxisStart + (1000 * rotAxis));

			//draw the speed counter
			_spriteBatch.DrawString(mainFont, $"Rotation Speed : {rotSpeed : 0.00}", new(10, 365), Color.Black); 

			_spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}