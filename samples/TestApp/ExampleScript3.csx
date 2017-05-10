using System;

class Test
{
    public string Test1 {get; set;}
    public double Test2 { get; set; }
}

class GenericTest<T>
{

}

var str = "Test";
str.ToCharArray();

var tst = new Test();
double dbl = tst.Test1 + 1;

Test test2 = new Test();
var gt = new GenericTest<Test>();
