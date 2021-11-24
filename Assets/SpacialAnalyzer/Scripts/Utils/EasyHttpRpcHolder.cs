/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2021 NekomimiDaimao
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * https://gist.github.com/nekomimi-daimao/e5726cde473de30a12273cd827779704
 * 
 */

using System.Collections.Specialized;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Nekomimi.Daimao
{
    public sealed class EasyHttpRpcHolder : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (Instance != null)
            {
                return;
            }

            var go = new GameObject(nameof(EasyHttpRpcHolder));
            DontDestroyOnLoad(go);
            var holder = go.AddComponent<EasyHttpRpcHolder>();
            Instance = holder;
            var easyHttp = new EasyHttpRPC(go.GetCancellationTokenOnDestroy());
            holder._easyHttpRPC = easyHttp;
            easyHttp.RegisterRPC(nameof(holder.Ping), holder.Ping);
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private static EasyHttpRpcHolder Instance = null;

        public static EasyHttpRPC EasyHttpRPC => Instance._easyHttpRPC;

        private EasyHttpRPC _easyHttpRPC = null;

        private UniTask<string> Ping(NameValueCollection arg)
        {
            var builder = new StringBuilder();
            builder.AppendLine(EasyHttpRPC.Address);
            foreach (var s in EasyHttpRPC.Registered)
            {
                builder.AppendLine(s);
            }
            return UniTask.FromResult(builder.ToString());
        }
    }
}
