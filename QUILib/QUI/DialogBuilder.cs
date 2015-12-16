using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Collections;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace QUI
{
    public interface IDialogBuilderCallback
    {
        ControlUI createControl(string className, PaintManagerUI manager = null);
    }
    public class DialogBuilder
    {
        public DialogBuilder()
        {
            mXML = new Markup();
        }
        ~DialogBuilder()
        {
            mXML.release();
            mXML = null;
        }

        public ControlUI createFromString(string strXML, IDialogBuilderCallback callback = null, PaintManagerUI manager = null)
        {
            mCallback = callback;
            if (mXML.loadFromString(ref strXML) == false)
            {
                return null;
            }

            MarkupNode root = mXML.getRoot();
            ControlUI control = null;

            parseWindowAttributes(ref root, ref manager);

            ControlUI rootNode = parse(ref root, ref control, ref manager);

            root.release();

            return rootNode;
        }
        public ControlUI createFromFile(string xmlFile, IDialogBuilderCallback callback = null, PaintManagerUI manager = null)
        {
            mCallback = callback;
            if (mXML.loadFromFile(xmlFile) == false)
            {
                return null;
            }

            MarkupNode root = mXML.getRoot();

            parseWindowAttributes(ref root, ref manager);

            ControlUI control = null;

            ControlUI rootNode = parse(ref root, ref control, ref manager);
            root.release();

            return rootNode;

        }
        public ControlUI createFromMem(ref char[] buffer, int count, IDialogBuilderCallback callback = null, PaintManagerUI manager = null)
        {
            if (mXML.loadFromMem(ref buffer, buffer.Length) == false)
            {
                return null;
            }

            MarkupNode root = mXML.getRoot();
            ControlUI control = null;

            parseWindowAttributes(ref root, ref manager);

            ControlUI rootNode = parse(ref root, ref control, ref manager);
            root.release();

            return rootNode;

        }

        protected bool parseWindowAttributes(ref MarkupNode root, ref PaintManagerUI manager)
        {
            if (manager != null)
            {
                string className = root.getName();
                if (className == "Window")
                {
                    List<MarkupNode> listNode = root.getChildList();
                    string imageName = "";
                    string imageResType = "";
                    uint mask = 0;
                    foreach (var node in listNode)
                    {
                        if (node.getName() == "Image")
                        {
                            List<XMLAttribute> listAttr = node.getAttributeList();
                            foreach (var attr in listAttr)
                            {
                                if (attr.getName() == "name")
                                {
                                    imageName = attr.getValue();
                                }
                                else if (attr.getName() == "restype")
                                {
                                    imageResType = attr.getValue();
                                }
                                else if (attr.getName() == "mask")
                                {
                                    string sMask = attr.getValue();
                                    sMask = sMask.TrimStart('#');
                                    mask = uint.Parse(sMask);
                                }
                            }

                            if (imageName != "")
                            {
                                manager.addImage(imageName, imageResType, (int)mask);
                            }
                        }
                        else if (node.getName() == "Font")
                        {
                            string fontName = "";
                            int size = 10;
                            bool bold = false;
                            bool underline = false;
                            bool italic = false;
                            bool defaultfont = false;
                            bool defaultboldfont = false;
                            bool defaultlinkfont = false;

                            List<XMLAttribute> listAttr = node.getAttributeList();

                            foreach (var attr in listAttr)
                            {
                                if (attr.getName() == "name")
                                {
                                    fontName = attr.getValue();
                                }
                                else if (attr.getName() == "size")
                                {
                                    size = int.Parse(attr.getValue());
                                }
                                else if (attr.getName() == "bold")
                                {
                                    bold = attr.getValue() == "true";
                                }
                                else if (attr.getName() == "underline")
                                {
                                    underline = attr.getValue() == "true";
                                }
                                else if (attr.getName() == "italic")
                                {
                                    italic = attr.getValue() == "true";
                                }
                                else if (attr.getName() == "default")
                                {
                                    defaultfont = attr.getValue() == "true";
                                }
                                else if (attr.getName() == "defaultbold")
                                {
                                    defaultboldfont = attr.getValue() == "true";
                                }
                                else if (attr.getName() == "defaultlink")
                                {
                                    defaultlinkfont = attr.getValue() == "true";
                                }
                            }

                            if (fontName != "")
                            {
                                Font font = manager.addFont(fontName, size-3, bold, underline, italic);
                                if (font != null)
                                {
                                    if (defaultfont)
                                    {
                                        manager.setDefaultFont(font);
                                    }
                                    if (defaultboldfont)
                                    {
                                        manager.setDefaultBoldFont(font);
                                    }
                                    if (defaultlinkfont)
                                    {
                                        manager.setDefaultLinkFont(font);
                                    }
                                }
                            }
                        }
                        else if (node.getName() == "Default")
                        {
                            List<XMLAttribute> listAttr = node.getAttributeList();
                            string ctlName = "";
                            string ctlValue = "";

                            foreach (var attr in listAttr)
                            {
                                if (attr.getName() == "name")
                                {
                                    ctlName = attr.getValue();
                                }
                                else if (attr.getName() == "value")
                                {
                                    ctlValue = attr.getValue();
                                }
                            }
                            if (ctlName != "")
                            {
                                manager.addDefaultAttributeList(ctlName, ctlValue);
                            }
                        }
                    }

                    if (manager.getPaintWindow() != null)
                    {
                        List<XMLAttribute> listAttr = root.getAttributeList();
                        foreach (var attr in listAttr)
                        {
                            if (attr.getName() == "size")
                            {
                                string[] listValue = attr.getValue().Split(',');
                                if (listValue.Length != 2)
                                {
                                    throw new Exception("窗口大小参数值有误");
                                }
                                int cx = int.Parse(listValue[0]);
                                int cy = int.Parse(listValue[1]);
                                manager.setInitSize(cx, cy);
                            }
                            else if (attr.getName() == "sizebox")
                            {
                                string[] listValue = attr.getValue().Split(',');
                                if (listValue.Length != 4)
                                {
                                    throw new Exception("窗口大小参数值有误");
                                }
                                int left = int.Parse(listValue[0]);
                                int top = int.Parse(listValue[1]);
                                int right = int.Parse(listValue[2]);
                                int bottom = int.Parse(listValue[3]);

                                Rectangle rect = new Rectangle(left, top, right - left, bottom - top);
                                manager.setSizeBox(ref rect);
                            }
                            else if (attr.getName() == "caption")
                            {
                                string[] listValue = attr.getValue().Split(',');
                                if (listValue.Length != 4)
                                {
                                    throw new Exception("标题大小参数值有误");
                                }
                                int left = int.Parse(listValue[0]);
                                int top = int.Parse(listValue[1]);
                                int right = int.Parse(listValue[2]);
                                int bottom = int.Parse(listValue[3]);

                                Rectangle rect = new Rectangle(left, top, right - left, bottom - top);
                                manager.setCaptionRect(ref rect);
                            }
                            else if (attr.getName() == "roundcorner")
                            {
                                string[] listValue = attr.getValue().Split(',');
                                if (listValue.Length < 2)
                                {
                                    throw new Exception("窗口边框圆角半径参数值有误");
                                }
                                int cx = int.Parse(listValue[0]);
                                int cy = int.Parse(listValue[1]);
                                manager.setRoundCorner(cx, cy);
                            }
                            else if (attr.getName() == "mininfo")
                            {
                                string[] listValue = attr.getValue().Split(',');
                                if (listValue.Length != 2)
                                {
                                    throw new Exception("窗口大小最小值参数值有误");
                                }
                                int cx = int.Parse(listValue[0]);
                                int cy = int.Parse(listValue[1]);
                                manager.setMinMaxInfo(cx, cy);
                            }
                            else if (attr.getName() == "showdirty")
                            {
                                manager.setShowUpdateRect(attr.getValue() == "true");
                            }
                        }
                    }
                }
            }

            return true;
        }

        public void getLastErrorMessage(ref string strMessage, int max)
        {

        }
        public void getLastErrorLocation(ref string strSource, int max)
        {

        }

        // 广度优先搜索XML节点树创建相应的控件树,过滤非控件节点
        protected ControlUI parse(ref MarkupNode parentNode, ref ControlUI parentControl, ref PaintManagerUI manager)
        {
            if (parentNode == null)
            {
                throw new Exception("对象未赋值");
            }
            ControlUI ctlParent = null;

            {
                //parentControl.setManager(manager, null);

                Queue<MarkupNode> queueNode = new Queue<MarkupNode>();
                Queue<ControlUI> queueControl = new Queue<ControlUI>();

                queueNode.Enqueue(parentNode);
                queueControl.Enqueue(parentControl);

                while (queueNode.Count > 0)
                {
                    MarkupNode curNode = queueNode.Dequeue();
                    ControlUI curParentControl = queueControl.Dequeue();
                    List<MarkupNode> listNode = curNode.getChildList();

                    // 访问根节点
                    if (listNode != null && listNode.Count > 0)
                    {
                        // 子节点入队
                        foreach (var node in listNode)
                        {
                            // 过滤非控件节点
                            if (node.getName() == "Window" ||
                                node.getName() == "Image" ||
                                node.getName() == "Font" ||
                                node.getName() == "Default")
                            {
                                continue;
                            }

                            ControlUI newControl = null;
                            {
                                queueNode.Enqueue(node);

                                // 创建控件，加入控件树后入队
                                newControl = getControl(node.getName());

                                if (newControl != null && newControl is ControlUI)
                                {
                                    queueControl.Enqueue(newControl);

                                    newControl.setManager(manager, curParentControl);
                                    if (curParentControl != null)
                                    {
                                        IContainerUI container = (IContainerUI)curParentControl.getInterface("IContainer");
                                        container.add(newControl);
                                    }
                                    else
                                    {
                                        if (ctlParent != null)
                                        {
                                            throw new Exception("不能有两个根容器");
                                        }
                                        ctlParent = newControl;
                                    }
                                }
                                else if (mCallback != null)
                                {
                                    newControl = mCallback.createControl(node.getName(), manager);
                                    if (newControl == null)
                                    {
                                        throw new Exception("未知控件类型");
                                    }
                                    queueControl.Enqueue(newControl);

                                    newControl.setManager(manager, curParentControl);
                                    if (curParentControl != null)
                                    {
                                        IContainerUI container = (IContainerUI)curParentControl.getInterface("IContainer");
                                        container.add(newControl);
                                    }
                                    else
                                    {
                                        if (ctlParent != null)
                                        {
                                            throw new Exception("不能有两个根容器");
                                        }
                                        ctlParent = newControl;
                                    }
                                }
                                else
                                {
                                    throw new Exception("未知控件类型");
                                }
                            }

                            {
                                // 设置属性
                                if (manager != null)
                                {
                                    //newControl.setManager(manager, curParentControl);
                                    string defaultAttributes = manager.getDefaultAttributeList(node.getName());
                                    if (defaultAttributes != "")
                                    {
                                        newControl.applyAttributeList(defaultAttributes);
                                    }
                                }

                                List<XMLAttribute> listAttr = node.getAttributeList();
                                foreach (var attr in listAttr)
                                {
                                    newControl.addAttribute(attr.getName(), attr.getValue());
                                    newControl.setAttribute(attr.getName(), attr.getValue());
                                }
                            }
                        }
                    }
                }
            }

            return ctlParent;
        }

        public static ControlUI getControl(string typeName)
        {
            ControlUI newControl = null;
            int len = typeName.Length;

            switch (len)
            {
                case 4:
                    {
                        if (typeName == "Edit")
                        {
                            newControl = new EditUI();
                        }
                        else if (typeName == "List")
                        {
                            newControl = new ListUI();
                        }
                        else if (typeName == "Text")
                        {
                            newControl = new TextUI();
                        }

                        break;
                    }
                case 5:
                    {
                        if (typeName == "Combo")
                        {
                            newControl = new ComboUI();
                        }
                        else if (typeName == "Label")
                        {
                            newControl = new LabelUI();
                        }
                        break;
                    }
                case 6:
                    {
                        if (typeName == "Button")
                        {
                            newControl = new ButtonUI();
                        }
                        else if (typeName == "Option")
                        {
                            newControl = new OptionUI();
                        }
                        else if (typeName == "Slider")
                        {
                            newControl = new SliderUI();
                        }

                        break;
                    }
                case 7:
                    {
                        if (typeName == "Control")
                        {
                            newControl = new ControlUI();
                        }
                        else if (typeName == "ActiveX")
                        {
                            newControl = new ActiveXUI();
                        }
                        break;
                    }
                case 8:
                    {
                        if (typeName == "Progress")
                        {
                            newControl = new ProgressUI();
                        }
                        break;
                    }
                case 9:
                    {

                        if (typeName == "Container")
                        {
                            newControl = new ContainerUI();
                        }
                        else if (typeName == "TabLayout")
                        {
                            newControl = new TabLayoutUI();
                        }

                        break;
                    }
                case 10:
                    {
                        if (typeName == "ListHeader")
                        {
                            newControl = new ListHeaderUI();
                        }
                        else if (typeName == "TileLayout")
                        {
                            newControl = new TileLayoutUI();
                        }

                        break;
                    }
                case 12:
                    {
                        if (typeName == "DialogLayout")
                        {
                            newControl = new DialogLayoutUI();
                        }
                        break;
                    }
                case 14:
                    {
                        if (typeName == "VerticalLayout")
                        {
                            newControl = new VerticalLayoutUI();
                        }
                        else if (typeName == "ListHeaderItem")
                        {
                            newControl = new ListHeaderItemUI();
                        }

                        break;
                    }
                case 15:
                    {
                        if (typeName == "ListTextElement")
                        {
                            newControl = new ListTextElementUI();
                        }
                        break;
                    }
                case 16:
                    {
                        if (typeName == "HorizontalLayout")
                        {
                            newControl = new HorizontalLayoutUI();
                        }
                        else if (typeName == "ListLabelElement")
                        {
                            newControl = new ListLabelElementUI();
                        }

                        break;
                    }
                case 17:
                    {
                        if (typeName == "ListExpandElement")
                        {
                            newControl = new ListExpandElementUI();
                        }
                        break;
                    }
                case 20:
                    {
                        if (typeName == "ListContainerElement")
                        {
                            newControl = new ListContainerElementUI();
                        }
                        break;
                    }

            }

            return newControl;
        }

        protected Markup mXML;
        protected IDialogBuilderCallback mCallback;
    }
}
