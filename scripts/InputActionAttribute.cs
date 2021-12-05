using System;

namespace JumpAndShoot.scripts
{
    internal class InputActionAttribute : Attribute
    {
        public string ActionName { get; }

        public InputActionAttribute(string actionName)
        {
            this.ActionName = actionName;
        }
    }
}