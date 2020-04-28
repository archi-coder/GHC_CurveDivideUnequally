using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace ArchiCoder
{
    public class ArchiCoderInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "ArchiCoder";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("6a3940e8-05d8-4c64-bf97-450994550035");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Filip Romaniuk";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "filipromaniuk@gmail.com";
            }
        }
    }
}
