﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScriptCore;
using System.Runtime.InteropServices;
using ScriptCore.Attributes;
using ScriptCore.Yielding;

namespace LuaGUI
{
    public partial class Form1 : Form
    {
        //Console show stuff
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        HookedStateScriptRunner testScriptRunner = null;

        BasicScriptRunner basicRunner = null;

        [LuaFunction("TestProp2")]
        public static Action<int> TestProp => (t) =>
        {
            Console.WriteLine($"Prop OK, passed: {t}");
        };

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            FormClosed += Form1_FormClosed;
            Yielders.RegisterYielder<MyYielder>("MyYielder");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            QuickScripting.RemoveBindings(new ScriptBindings(this));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
            Console.WriteLine("Is 64 bit process: " + Environment.Is64BitProcess.ToString());
            //Basic runner test
            basicRunner = new BasicScriptRunner((Action<string>)PrintPlusA);
            basicRunner.Run("PrintPlusA('Hi this is a test from Lua!')");
        }

        //[LuaDocumentation("Prints (A) + value to the console")]
        //[LuaExample("PrintPlusA('Hello World')")]
        //[LuaFunction("PrintPlusA")]
        public void PrintPlusA(string s)
        {
            Console.WriteLine("(A) " + s);
        }

        private void bTest_Click(object sender, EventArgs e)
        {
           
        }

        private void bStart_Click(object sender, EventArgs e)
        {
            testScriptRunner = new HookedStateScriptRunner(new ScriptBindings(this));
            bExecute.Enabled = true;
            bDispose.Enabled = true;
            bAbort.Enabled = true;
            bSetStashkey.Enabled = true;
            bStart.Enabled = false;
            bCallHook.Enabled = true;
        }

        private void bExecute_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine($"LOOP ============================");
                Console.WriteLine($"(C#) PreExecute");
                testScriptRunner.Execute("PreExecute");
                Console.WriteLine($"\r\n(C#) Execute");
                testScriptRunner.Execute("Execute");
                Console.WriteLine($"\r\n(C#) PostExecute");
                testScriptRunner.Execute("PostExecute");
            }
            catch 
            {
                Console.WriteLine("ERROR: Script was aborted!");
                bDispose_Click(null, null);
            }
        }

        private void bDispose_Click(object sender, EventArgs e)
        {
            testScriptRunner = null;
            bExecute.Enabled = false;
            bDispose.Enabled = false;
            bAbort.Enabled = false;
            bStart.Enabled = true;
            bSetStashkey.Enabled = false;
            bCallHook.Enabled = false;

        }

        private void bAbort_Click(object sender, EventArgs e)
        {
            //testScriptRunner.Abort();
        }

        private void bSetStashkey_Click(object sender, EventArgs e)
        {
            testScriptRunner?.LoadScript(tbScript.Text);
            Console.WriteLine($"Loaded script");
        }

        private void bCallHook_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"Executing {tbHook.Text}");
            testScriptRunner?.Execute(tbHook.Text);
        }

        private void bLoadKeyCoroutine_Click(object sender, EventArgs e)
        {
            testScriptRunner?.Execute("OnStashkeyLoad", 1, "help");
        }
    }

    class A
    {

    }

    class MyYielder : Yielder
    {
        public MyYielder(int a, string b)
        {
            Console.WriteLine("Custom Int" + a.ToString());
            Console.WriteLine("Custom String" + b);
        }

        public override bool CheckStatus()
        {
            return true;
        }
    }

}
