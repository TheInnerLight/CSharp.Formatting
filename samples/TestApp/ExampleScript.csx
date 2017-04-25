/***
Write some documentation here...
*/

// Needed for System.Linq reference
#r "System.Core" 

using System;
using System.Collections.Generic;
using System.Linq;

public class MyTestAttribute : Attribute 
{

}

/***
Now use the Attribute...
*/

public class TestClass 
{
    [MyTest]
    public static void Test(IDisposable[] test) 
    {

    }

    public static void main(string[] args)
    {
        try
        {
            // something exceptional
        }
        catch(Exception ex) 
        {
            throw;
        }

        string helloWorld = "Hello World!";
        Console.WriteLine(helloWorld);

        IEnumerable<int> test = Enumerable.Range(0,50);
    }
}
