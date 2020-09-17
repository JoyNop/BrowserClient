using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chromium;
using Chromium.Remote;
using NetDimension.NanUI;
using NetDimension.NanUI.Browser;
namespace JoyNop_Windows
{
    class MainWindows : Formium
    {

        string aurl = "http://127.0.0.1:5500/index.html";
        String burl = "http://192.168.1.125:3000";
        public override string StartUrl => burl;

        public override HostWindowType WindowType => HostWindowType.Standard;

        protected override Control LaunchScreen => null;

        public MainWindows()
        {
            Title = "第一个应用";
        }


        // 浏览器核心准备就绪
        protected override void OnWindowReady(IWebBrowserHandler browserClient)
        {
           // WebBrowser.ShowDevTools();
            // 在此处添加CEF客户端各项行文的处理子程序，例如：下载（通常使用DownloadHandler处理下载过程）、弹窗（LiftSpanHandler）、信息显示（DisplayHandler）等等。
        }



        // 浏览器Javascript环境初始化完成
        protected override void OnRegisterGlobalObject(JSObject global)
        {
            // 可以在此处将C#对象注入到当前窗口的JS上下文中



            //add a function with callback function
            Console.Write(global);
            var callbackTestFunc = global.AddFunction("callbackTest");
            callbackTestFunc.Execute += (func, args) => {
                var callback = args.Arguments.FirstOrDefault(p => p.IsFunction);
                if (callback != null)
                {

                    WebBrowser.ExecuteJavascript("ChangeTitle()");


                    var callbackArgs = CfrV8Value.CreateObject(new CfrV8Accessor());
                    callbackArgs.SetValue("success", CfrV8Value.CreateBool(true), CfxV8PropertyAttribute.ReadOnly);
                    callbackArgs.SetValue("text", CfrV8Value.CreateString("Message from Windows Client"), CfxV8PropertyAttribute.ReadOnly);

                    callback.ExecuteFunction(null, new CfrV8Value[] { callbackArgs });


                }
            };



            //register the "my" object
            var myObject = global.AddObject("my");

            //add property "name" to my, you should implemnt the getter/setter of name property by using PropertyGet/PropertySet events.
            var nameProp = myObject.AddDynamicProperty("name");
            nameProp.PropertyGet += (prop, args) =>
            {
                 string computerName = Environment.MachineName;

                string value = $"My Computer Name is JoyNop :)\n{computerName}";

                // getter - if js code "my.name" executes, it'll get the string "NanUI".
                args.Retval = CfrV8Value.CreateString(value);
                args.SetReturnValue(true);
            };
            nameProp.PropertySet += (prop, args) =>
            {
                // setter's value from js context, here we do nothing, so it will store or igrone by your mind.
                var value = args.Value;
                args.SetReturnValue(true);
            };


            //add a function showCSharpMessageBox
            var showMessageBoxFunc = myObject.AddFunction("showCSharpMessageBox");
            showMessageBoxFunc.Execute += (func, args) =>
            {
                //it will be raised by js code "my.showCSharpMessageBox(`some text`)" executed.
                //get the first string argument in Arguments, it pass by js function.
                var stringArgument = args.Arguments.FirstOrDefault(p => p.IsString);

                if (stringArgument != null)
                {
                    string osVersionName = Environment.OSVersion.ToString();
                    MessageBox.Show(osVersionName, "Windows 内核版本",MessageBoxButtons.OK,MessageBoxIcon.Information );


                }
            };


            var friends = new string[] { "Mr.JSON", "Mr.Lee", "Mr.BONG" };
            var getObjectFormCSFunc = myObject.AddFunction("getObjectFromCSharp");
            getObjectFormCSFunc.Execute += (func, args) =>
            {
                //create the CfrV8Value object and the accssor of this Object.
                var jsObjectAccessor = new CfrV8Accessor();
                var jsObject = CfrV8Value.CreateObject(jsObjectAccessor);

                //create a CfrV8Value array
                var jsArray = CfrV8Value.CreateArray(friends.Length);

                for (int i = 0; i < friends.Length; i++)
                {
                    jsArray.SetValue(i, CfrV8Value.CreateString(friends[i]));
                }

                jsObject.SetValue("libName", CfrV8Value.CreateString("NanUI"), CfxV8PropertyAttribute.ReadOnly);
                jsObject.SetValue("friends", jsArray, CfxV8PropertyAttribute.DontDelete);


                args.SetReturnValue(jsObject);

                //in js context, use code "my.getObjectFromCSharp()" will get an object like { friends:["Mr.JSON", "Mr.Lee", "Mr.BONG"], libName:"NanUI" }
            };




        }

        // 在此处定义标准窗口的基础样式
        protected override void OnStandardFormStyle(IStandardHostWindowStyle style)
        {
            base.OnStandardFormStyle(style);

            style.Width = 1280;
            style.Height = 720;
            style.Icon = System.Drawing.SystemIcons.WinLogo;
            style.StartPosition = FormStartPosition.CenterScreen;
        }



    }
}
