class BlurCubeMapWindow extends EditorWindow {

	// Add menu
	@MenuItem ("Window/Blur Cube Map")		// change name of menu here
	static function BlurCubeMapMenu() {
		EditorWindow.GetWindow (BlurCubeMapWindow, true, "Blur Cube Map");
	}

	private var blurAngle : float = 5;
	private var peak : float = 0;
	private var samps : int = 128;
	private var brightness : float = 0.0;
	private var gamma : float = 0.0;
	private var size : int = 64;
	private var encoding : int = 1;
	private var possibleSizes : int[] = [16, 32, 64, 128, 256, 512, 1024, 2048];
	private var gpu : boolean = true;

	// creates and refreshes the GUI
	function OnGUI () {
		var hasPro : boolean = UnityEditorInternal.InternalEditorUtility.HasPro();

		blurAngle = EditorGUILayout.FloatField("Blur Angle in Degrees:", Mathf.Clamp(blurAngle, 0.0, 180.0));
		peak = EditorGUILayout.FloatField("Peak (Default is 0):", Mathf.Clamp(peak, 0.0, 5.0));

		GUILayout.Space(10);

		brightness = EditorGUILayout.FloatField("Brightness:", brightness);
		gamma = EditorGUILayout.FloatField("Gamma:", gamma);
		
		GUILayout.Space(10);
				
		size = EditorGUILayout.IntPopup("Face Size: ", size, ["16", "32", "64", "128", "256", "512", "1024", "2048"], possibleSizes);
		encoding = EditorGUILayout.IntPopup("Encoding: ", encoding, ["(Alpha8) ALPHA-only texture format", "(RGB24) COLOR texture format.", "(ARGB32) COLOR with ALPHA texture format."], [0,1,2,3]);   	
		gpu = EditorGUILayout.Toggle("Use GPU: ", gpu);
		if(!gpu) samps = EditorGUILayout.IntField("Nr of Samples: ", samps);

		GUILayout.Space(20);

		var sel = Selection.activeObject;				// take first object of selection
		var path : String;

		var valid : int = 0;		// check if the selection is valid
		if(sel == null) valid = 0;
		else
			if(gpu && !hasPro) valid = 2;
			else
				if(sel.GetType() != Cubemap) valid = 1;
				else
					{
					valid = 4;
					path = AssetDatabase.GetAssetPath(sel).Split("."[0])[0];		// name from the selected Cubemap
					path += "_B" + blurAngle + "_P" + peak + ".cubemap";
					if(!gpu)
						{
						var e : UnityException;
						try sel.GetPixel(CubemapFace.PositiveX, 1, 1);				// try if cubemap is readable (only needed if rendering on CPU)
						catch (e) valid = 3;
						}
					}

		if(GUILayout.Button ("Render") && (valid == 4)) 
			{
			var dest : Cubemap;
			if(gpu) dest = BlurCubeMapGPU(sel, size, encoding, blurAngle, peak, brightness, gamma);
			else dest = BlurCubeMap(sel, size, encoding, blurAngle, peak, samps, brightness, gamma);
			AssetDatabase.CreateAsset(dest, path);		// write destination cubemap to disk
			}

		switch(valid)
			{
			case 0 : GUILayout.Label("Nothing Selected"); break;
			case 1 : GUILayout.Label("Select a Cubemap in the Project Window."); break;
			case 2 : GUILayout.Label("GPU rendering not supported in Unity Free."); break;
			case 3 : GUILayout.Label("Cubemap needs to be readable."); break;
			case 4 : GUILayout.Label("Will save to: " + path); break;
			}
	}



	static function BlurCubeMapGPU(source : Cubemap, size : int, encoding : int, angle : float, peak : float, brightness : float, gamma : float) : Cubemap {
		var dest : Cubemap;		// the destination cubemap
		switch(encoding)		// create new empty cubemap with correct encoding
			{
			case 0 : dest = new Cubemap (size,  TextureFormat.Alpha8, false); break;
			case 1 : dest = new Cubemap (size,  TextureFormat.RGB24, false); break;
			case 2 : dest = new Cubemap (size,  TextureFormat.ARGB32, false); break;			
			}
		
		var dir = new Vector3[6];		// store all possible direction vectors and their corresponding CubemapFace into two arrays
		dir[0] = Vector3(1,0,0); dir[1] = Vector3(0,1,0); dir[2] = Vector3(0,0,1); dir[3] = Vector3(-1,0,0); dir[4] = Vector3(0,-1,0); dir[5] = Vector3(0,0,-1);
		var facing = new CubemapFace[6];
		facing[0] = CubemapFace.PositiveX; facing[1] = CubemapFace.PositiveY; facing[2] = CubemapFace.PositiveZ; facing[3] = CubemapFace.NegativeX; facing[4] = CubemapFace.NegativeY; facing[5] = CubemapFace.NegativeZ;
		
		var renderTarget : RenderTexture = new RenderTexture(size,size, 0);			// this is the texture that will be rendered to
		var textureTarget = new Texture2D(size, size, TextureFormat.ARGB32, false);	// this is a helper Texture2D to get pixel values from the renderTexture
		var rndNumbers = new Texture2D(512, 512, TextureFormat.ARGB32, false);		// this is a 512x512 texture, filled with random numbers
		
		var sideColors : Color[];
		
		//prepare the Material, that does the calculation on the GPU
		var calcMaterial : Material = new Material (Shader.Find("Custom/BlurCubeMapSide"));
		calcMaterial.SetTexture("_LookupCube", source);
		calcMaterial.SetFloat("_Angle", angle);
		calcMaterial.SetFloat("_Peak", peak);
		calcMaterial.SetFloat("_Size", size);
		calcMaterial.SetFloat("_Brigthness", brightness + 1);
		calcMaterial.SetFloat("_Gamma", (gamma != -1) ? (1.0 / (gamma + 1.0)) : 1);
		calcMaterial.SetTexture("_RndNumbers", rndNumbers);
		
		var uAxis : Vector3;
		var vAxis : Vector3;
		
		// fill rndNumbers texture with random values
		rndNumbers.anisoLevel = 0;
		rndNumbers.filterMode = FilterMode.Point;
		for(var p = 0; p < 512 * 512; p++)
			{
			rndNumbers.SetPixel(p % 512, Mathf.Floor(p / 512), FloatToColor(UnityEngine.Random.Range(0.0, 1.0)));
			}
		rndNumbers.Apply();
			
		// calculate each side of the Cubemap individually
		for(var i = 0; i < 6; i++)
			{
			switch (dir[i])		// find out which axis are needed
				{
				 case Vector3(1,0,0):  uAxis = Vector3(0,0,-1); vAxis = Vector3(0,-1,0); break;
				 case Vector3(-1,0,0): uAxis = Vector3(0,0,1);  vAxis = Vector3(0,-1,0); break;
				 case Vector3(0,1,0):  uAxis = Vector3(1,0,0);  vAxis = Vector3(0,0,1);  break;
				 case Vector3(0,-1,0): uAxis = Vector3(1,0,0);  vAxis = Vector3(0,0,-1); break;
				 case Vector3(0,0,1):  uAxis = Vector3(1,0,0);  vAxis = Vector3(0,-1,0); break;
				 case Vector3(0,0,-1): uAxis = Vector3(-1,0,0); vAxis = Vector3(0,-1,0); break;
				}
			
			calcMaterial.SetVector("_Direction", dir[i]);
			calcMaterial.SetVector("_UAxis", uAxis);
			calcMaterial.SetVector("_VAxis", vAxis);
			
			Graphics.Blit(textureTarget, renderTarget, calcMaterial);			// calculate this side on the GPU
			
			RenderTexture.active = renderTarget;
			textureTarget.ReadPixels(new Rect(0, 0, size, size), 0,0, false);	// copy renderTexture to a Texture2D
			textureTarget.Apply();
			
			sideColors = textureTarget.GetPixels();		// copy the colors into an array
			
			dest.SetPixels(sideColors, facing[i], 0);	// write the colors to the correct side of the cubemap
			}		
		dest.Apply();		// apply all pixels to destination cubemap   	
		return dest;
	}
	

	static function BlurCubeMap(source : Cubemap, size : int, encoding : int, angle : float, peak : float, samps : int, brightness : float, gamma : float) : Cubemap {
		var dest : Cubemap;		// the destination cubemap
		switch(encoding)		// create new empty cubemap with correct encoding
			{
			case 0 : dest = new Cubemap (size,  TextureFormat.Alpha8, false); break;
			case 1 : dest = new Cubemap (size,  TextureFormat.RGB24, false); break;
			case 2 : dest = new Cubemap (size,  TextureFormat.ARGB32, false); break;			
			}
		
		var dir = new Vector3[6];		// store all possible direction vectors and their corresponding CubemapFace into two arrays
		dir[0] = Vector3(1,0,0); dir[1] = Vector3(0,1,0); dir[2] = Vector3(0,0,1); dir[3] = Vector3(-1,0,0); dir[4] = Vector3(0,-1,0); dir[5] = Vector3(0,0,-1);
		var facing = new CubemapFace[6];
		facing[0] = CubemapFace.PositiveX; facing[1] = CubemapFace.PositiveY; facing[2] = CubemapFace.PositiveZ; facing[3] = CubemapFace.NegativeX; facing[4] = CubemapFace.NegativeY; facing[5] = CubemapFace.NegativeZ;
		
		
		var directionVector : Vector3;		// the direction that will be sampled
		var tanu : Vector3;
		var tanv : Vector3;
		var sampleVector : Vector3;			// the direction of an individual sample
		var sampleColor : Color;			// the color returned from sampled direction
		var uCoord : float;					// the u and v coordinates of the destination cubemap
		var vCoord : float;
		

		// generate all Vectors, go through all axis (i), and all u and v coordinates (uCoord, vCoord) on the destination cubemap
		for(var i = 0; i < 6; i++)		// for each side of the cube
			for(uCoord = 0; uCoord < size; uCoord++)
				for(vCoord = 0; vCoord < size; vCoord++)
					{
					directionVector = CalcDirection(dir[i], (uCoord+0.5) / size * 2 - 1, (vCoord+0.5)  / size * 2 - 1).normalized;		// calculate the sample Vector from the cube coordinates					
					// create arbitrary tangent vectors
					tanu = Vector3.Cross(directionVector, Vector3(0.0,1.0,0.0)).normalized;
					tanv = Vector3.Cross(directionVector, tanu).normalized;
		
					sampleColor = Color.black;
					for(var s = 0; s < samps; s++)
						{
						sampleVector = SampleSphere(tanu, tanv, directionVector, angle, peak).normalized;	// find a random sampleVector around directionVector
						sampleColor += ReadCubeMap(source, sampleVector , source.height);	 				// sample this direction and add to color
						}
					sampleColor /= samps;																	// average all sampled colors
					
					sampleColor *= (brightness + 1.0);														// adjust brigthness
					if(gamma != -1.0) sampleColor = ColorPow(sampleColor, 1.0 / (gamma + 1.0));				// adjust gamma
					sampleColor = ColorPow(sampleColor, 0.454);
					dest.SetPixel(facing[i], uCoord, vCoord, sampleColor );									// write pixel to destination cubemap
					}
		dest.Apply();		// apply all pixels to destination cubemap   	
		return dest;
	}	
		
		
	// function to encode Float into RGB (to write them to a texture for use in the shader)
	static function FloatToColor(unit : float) : Color {
		var color : Color = Color(1, 255, 65025);
		color *= unit;
		color.g = color.g % 1;
		color.b = color.b % 1;
		color.r -= color.g * 0.00390625;
		color.g -= color.b * 0.00390625;
		color.r = Mathf.Clamp01(color.r);
		color.g = Mathf.Clamp01(color.g);
		color.b = Mathf.Clamp01(color.b);
		return color;
	}
	
	
	// helper function for gamma correction
	static function ColorPow(c : Color, pow : float) : Color	{
		c.r = Mathf.Pow(c.r, pow);
		c.g = Mathf.Pow(c.g, pow);
		c.b = Mathf.Pow(c.b, pow);
		c.a = Mathf.Pow(c.a, pow);
		return c;
		}
	
	
	// this Function calculates the direction (vector) given u and v coordinates (between -1 and 1) and the direction of the axis
	static function CalcDirection(dir : Vector3, u : float, v : float) : Vector3	{
		var uAxis : Vector3;
		var vAxis : Vector3;
		
		// determine correct u and v Vectors (axis)
		switch (dir)
			{
			 case Vector3(1,0,0): uAxis = Vector3(0,0,-1); vAxis = Vector3(0,-1,0); break;
			 case Vector3(-1,0,0): uAxis = Vector3(0,0,1); vAxis = Vector3(0,-1,0); break;
			 case Vector3(0,1,0): uAxis = Vector3(1,0,0); vAxis = Vector3(0,0,1); break;
			 case Vector3(0,-1,0): uAxis = Vector3(1,0,0); vAxis = Vector3(0,0,-1); break;
			 case Vector3(0,0,1): uAxis = Vector3(1,0,0); vAxis = Vector3(0,-1,0); break;
			 case Vector3(0,0,-1): uAxis = Vector3(-1,0,0); vAxis = Vector3(0,-1,0); break;
			}
		return (uAxis * u + vAxis * v + dir);		// not normalized!
	}
	
	
	// this Function takes a cubemap and a vector and returns the color, similar to texCUBE in a shader
	static function ReadCubeMap(source : Cubemap, v : Vector3, size : int) : Color	{
		// possible axis
		var axis = new Vector3[3];	
		if(v.x > 0) axis[0] = Vector3(1,0,0);
			else axis[0] = Vector3(-1,0,0);
		if(v.y > 0) axis[1] = Vector3(0,1,0);
			else axis[1] = Vector3(0,-1,0);
		if(v.z > 0) axis[2] = Vector3(0,0,1);
			else axis[2] = Vector3(0,0,-1);	
				
				
		// biggest dot product is direction we're facing
		var dot : float = 0.0;				
		var facing : Vector3;		// the direction v is Facing
		var tmp : float;
		for(var i = 0; i < 3; i++)
			{
			tmp = Vector3.Dot( axis[i], v);
			if(tmp > dot)
				{
					dot = tmp;
					facing = axis[i];
				}
			}
				
		var length : float = 1 / dot;	// length of the vector v assuming we're inside a box the size of 1
		var uv : Vector3 = v * length - facing;		// uv coordinates in 3D space -1 to 1
		var col : Color;
		
		uv = (uv / 2 + Vector3(0.5,0.5,0.5)) * size;			// 0 to size range 
		if(facing == Vector3(1,0,0)) col = source.GetPixel(CubemapFace.PositiveX, size - uv.z, size - uv.y);
		if(facing == Vector3(-1,0,0)) col = source.GetPixel(CubemapFace.NegativeX, uv.z, size - uv.y);
		if(facing == Vector3(0,1,0)) col = source.GetPixel(CubemapFace.PositiveY, uv.x, uv.z);
		if(facing == Vector3(0,-1,0)) col = source.GetPixel(CubemapFace.NegativeY, uv.x, size - uv.z);
		if(facing == Vector3(0,0,1)) col = source.GetPixel(CubemapFace.PositiveZ, uv.x, size - uv.y);
		if(facing == Vector3(0,0,-1)) col = source.GetPixel(CubemapFace.NegativeZ, size - uv.x, size - uv.y);
		
		return ColorPow(col, 2.2);		// return linearized color
	}


	// this function takes a Vector (the sample direction), angle and peak. And returns one random sample vector
	static function SampleSphere(tanu : Vector3, tanv : Vector3, dir : Vector3, angle : float, peak : float) : Vector3 {
		// random numbers
		var sx : float = UnityEngine.Random.Range(0.0, 1.0);
		var sy : float = UnityEngine.Random.Range(0.0, 1.0);
		var sz : float = UnityEngine.Random.Range(0.0, 1.0);
		
		// incorporate peak into n, 
		angle = (Mathf.Pow(sz, peak)) * angle;
		var n : float = AngleToExp(angle);
		
		var phi : float = sx * 6.28318530718;
		var theta : float = Mathf.Acos( Mathf.Pow (1 - sy, 1 / (n + 1)));	
		var sinth : float = Mathf.Sin(theta);
		var Sh : Vector3 = Mathf.Cos(phi) * sinth * tanu + Mathf.Sin(phi) * sinth * tanv + Mathf.Cos(theta) * dir;
		return -dir + 2 * Vector3.Dot(dir,Sh) * Sh;	 // calculating sample vector
	}
	
	
 // converts angle from UI to exponent for SampleSphere
	static function AngleToExp(angle : float) : float {
		return 6.55 + (8000000 / Mathf.Pow(1 + Mathf.Pow( angle / 23, 0.33), 13.8));
	}	
}		// end class


