using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	public class MMSpringRotation : MMSpringVector3Component<Transform>
	{
		public enum Spaces { Local, World }

		[MMInspectorGroup("Target")] 
		public Spaces Space = Spaces.World;
		
		public override Vector3 TargetVector3
		{
			get => (Space == Spaces.Local) ? Target.localRotation.eulerAngles : Target.rotation.eulerAngles;
			set
			{
				if (Space == Spaces.Local)
				{
					Target.localRotation = Quaternion.Euler(value);
				}
				else
				{
					Target.rotation = Quaternion.Euler(value);
				}
			}
		}
	}
}
