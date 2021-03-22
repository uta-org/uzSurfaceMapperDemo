using System.Collections;

namespace uzSurfaceMapper.Core
{
    public interface IInvoke
    {
        void InvokeAtAwake();

        void InvokeAtStart();

        IEnumerator InvokeAtStartAsEnumerator();

        void InvokeAtUpdate();

        void InvokeAtGUI();
    }
}