
function Update () 
	{

			
		if ( Input.GetKey(KeyCode.LeftArrow))
			{
				transform.Rotate( 0,Time.deltaTime*100,0);
			}
		
		if (Input.GetKey(KeyCode.RightArrow))
			{
				transform.Rotate(0,Time.deltaTime*-100,0);
			}
	}
	
