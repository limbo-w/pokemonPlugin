using UnityEngine;
using UnityEngine.UI;

namespace Chibi.Free
{

    public class DialogButton : Button
    {

        public Dialog parent;
        public int index;

        public void OnClick()
        {
            parent.OnClickButton(index);
        }

    }

}