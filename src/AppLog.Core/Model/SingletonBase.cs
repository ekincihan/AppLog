namespace AppLog.Core.Model
{
    public class SingletonBase<T> where T : class, new()
    {
        private static readonly object padlock = new object();
        private static T _instance;

        public static T Instance
        {
            get
            {
                lock (padlock)
                {

                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                    return _instance;
                }
            }
        }
    }
}