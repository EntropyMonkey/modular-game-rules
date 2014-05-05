using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModularRules
{
	public enum Direction { FORWARD = 0, BACKWARD, LEFT, RIGHT, UP, DOWN }

	public enum RelativeTo { WORLD = 0, ACTOR, SELF }

	public enum CollisionPhase { ENTER, STAY, EXIT, ANY }

	public enum Comparison { GREATER, EQUAL, LESS }
}
