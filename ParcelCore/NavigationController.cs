using System;

namespace ParcelCore
{
    public class NavigationController
    {
        public NavigationTarget CurrentTarget { get; private set; } = NavigationTarget.Dashboard;

        public event Action<NavigationTarget>? NavigationChanged;

        public void NavigateTo(NavigationTarget target)
        {
            if (CurrentTarget != target)
            {
                CurrentTarget = target;
                NavigationChanged?.Invoke(target);
            }
        }
    }
}
