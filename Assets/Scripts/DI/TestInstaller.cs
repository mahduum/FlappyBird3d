using UnityEngine;
using Zenject;

namespace DI
{
    public class TestInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<StringWrapper>().AsSingle();
            Container.Bind<Greeter>().AsSingle().NonLazy();
        }
        
        public class Greeter
        {
            public Greeter(IStringWrapper message)
            {
                Debug.Log(message.Message);
            }
        }

        public class StringWrapper : IStringWrapper
        {
            public string Message { get; } = "String Wrapper!";

            public void SomeMethod()
            {
            }
        }
    }

    public interface IStringWrapper
    {
        string Message { get; }
    }
}