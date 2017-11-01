using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace QUI
{
    public class XMLAttribute
    {
        public XMLAttribute(string name, string value)
        {
            mAttrName = name;
            mAttrValue = value;
        }
        ~XMLAttribute()
        {

        }
        public string getName()
        {
            return mAttrName;
        }
        public string getValue()
        {
            return mAttrValue;
        }
        public bool setName(string attrName)
        {
            mAttrName = attrName;

            return true;
        }
        public bool setValue(string value)
        {
            mAttrValue = value;

            return true;
        }

        protected string mAttrName;
        protected string mAttrValue;
    }
    public class MarkupNode
    {
        public MarkupNode(MarkupNode parent)
        {
            mNodeName = "";
            mParentNode = parent;
            mListChildNode = new List<MarkupNode>();
            mListAttribute = new List<XMLAttribute>();

            {
                if (parent != null)
                {
                    List<MarkupNode> listChild = parent.getChildList();
                    listChild.Add(this);
                }
            }
        }
        ~MarkupNode()
        {
            release();
        }
        public string getName()
        {
            return mNodeName;
        }
        public string getValue()
        {
            return mValue;
        }
        public MarkupNode getParent()
        {
            return mParentNode;
        }
        public List<MarkupNode> getChildList()
        {
            return mListChildNode;
        }
        public List<XMLAttribute> getAttributeList()
        {
            return mListAttribute;
        }
        public XMLAttribute this[string attrName]
        {
            get
            {
                if (attrName == "")
                {
                    throw new Exception("属性名不能为空");
                }

                return getAttribute(attrName);
            }
        }
        public XMLAttribute getAttribute(string attrName)
        {
            if (attrName == "")
            {
                throw new Exception("属性名不能为空");
            }
            foreach (var attr in mListAttribute)
            {
                if (attr.getName() == attrName)
                {
                    return attr;
                }
            }

            return null;
        }
        public MarkupNode getSibling(int idx)
        {
            if (idx < 0 || idx > mListChildNode.Count())
            {
                throw new Exception("索引越界!");
            }
            MarkupNode result = mParentNode == null ? null : mParentNode.getChild(idx);

            return result;
        }
        public MarkupNode getSibling(MarkupNode curChildNode)
        {
            for (int i = 0; i < mListChildNode.Count(); i++)
            {
                if (mListChildNode[i] == curChildNode)
                {
                    return i + 1 < mListChildNode.Count() ? mListChildNode[i + 1] : null;
                }
            }

            return null;
        }
        public MarkupNode getSibling()
        {
            if (mParentNode == null)
            {
                return null;
            }
            List<MarkupNode> listChild = mParentNode.getChildList();
            for (int i = 0; i < listChild.Count(); i++)
            {
                if (listChild[i] == this)
                {
                    return i + 1 < listChild.Count() ? listChild[i + 1] : null;
                }
            }

            return null;
        }
        public int getSiblingCount()
        {
            int result = mParentNode == null ? 0 : mParentNode.getChildCount() - 1;

            return result;
        }
        public MarkupNode getChild(int idx)
        {
            if (idx < 0 || idx > mListChildNode.Count())
            {
                throw new Exception("索引越界!");
            }
            return mListChildNode[idx];
        }
        public int getChildCount()
        {
            return mListChildNode.Count();
        }

        public bool release()
        {
            if (mListChildNode != null)
            {
                mListChildNode.Clear();
            }
            if (mListAttribute != null)
            {
                mListAttribute.Clear();
            }

            mParentNode = null;

            return true;
        }
        public bool removeSibling(int idx)
        {
            if (idx >= mListChildNode.Count())
            {
                return false;
            }
            MarkupNode node = getSibling(idx);

            mListChildNode.Remove(node);

            return true;
        }
        public bool setName(string nodeName)
        {
            mNodeName = nodeName;

            return true;
        }
        public bool setValue(string value)
        {
            mValue = value;

            return true;
        }
        public bool removeChild(int idx)
        {
            if (idx >= mListChildNode.Count())
            {
                return false;
            }
            MarkupNode node = mListChildNode[idx];

            mListChildNode.Remove(node);

            return true;
        }

        protected MarkupNode mParentNode;
        protected string mNodeName;
        protected string mValue;
        protected List<MarkupNode> mListChildNode;
        protected List<XMLAttribute> mListAttribute;
    }

    public class Markup
    {
        public Markup()
        {
            mRootNode = new MarkupNode(null);
        }
        ~Markup()
        {
            release();
        }
        public MarkupNode getRoot()
        {
            return mRootNode;
        }
        public bool loadFromString(string strXML)
        {
            int xmlLen = strXML.Length;
            char[] buffer = new char[xmlLen + sizeof(char)];
            int i = 0;
            foreach (var ch in strXML)
            {
                buffer[i] = ch;
                i++;
            }
            buffer[xmlLen] = (char)0;

            parse(ref buffer, xmlLen, ref mRootNode);

            buffer = null;

            return true;
        }
        public bool loadFromFile(string fileName, PaintManagerUI manager = null)
        {
            if (manager != null && manager.hasPackageCache())
            {
                MemoryStream cache = manager.getFileFromPackage(fileName);

                if (cache == null)
                {
                    throw new Exception("");
                }

                {
                    StreamReader readerXML = new StreamReader(cache);
                    int fileSize = (int)cache.Length;
                    char[] buffer = new char[fileSize + sizeof(char)];

                    readerXML.Read(buffer, 0, fileSize);
                    buffer[fileSize] = (char)0;

                    readerXML.Close();
                    readerXML = null;

                    parse(ref buffer, fileSize, ref mRootNode);

                    buffer = null;
                }

                cache.Close();
                cache.Dispose();
                cache = null;

                return true;
            }
            else if (manager != null && manager.getWorkingDir() != "")
            {
                fileName = manager.getWorkingDir() + fileName;
            }

            if (File.Exists(fileName) == false)
            {
                throw new Exception("");
            }

            {
                FileStream fileXML = new FileStream(fileName, FileMode.Open);
                StreamReader sr = new StreamReader(fileXML);
                int fileSize = (int)fileXML.Length;
                char[] buffer = new char[fileSize + sizeof(char)];

                sr.Read(buffer, 0, fileSize);
                buffer[fileSize] = (char)0;

                sr.Close();
                sr.Dispose();
                sr = null;
                fileXML = null;

                parse(ref buffer, fileSize, ref mRootNode);

                buffer = null;
            }

            return true;
        }
        public bool loadFromMem(ref char[] buffer, int count)
        {
            parse(ref buffer, count, ref mRootNode);

            return true;
        }
        public bool parse(ref char[] buffer, int count, ref MarkupNode rootNode)
        {
            if (buffer == null || count < 0 || count > buffer.Count())
            {
                throw new Exception("缓存为空或者索引越界");
            }
            if (count == 0)
            {
                return false;
            }

            {
                int curIdx = 0;

                Stack<MarkupNode> stackNode = new Stack<MarkupNode>();
                MarkupNode newNode = null;
                stackNode.Push(rootNode);

                while (stackNode.Count() > 0)
                {
                    MarkupNode curNode = stackNode.Peek();

                    {
                        // 解析结束标签
                        curIdx = skipWhiteSpace(ref buffer, curIdx);
                        if (buffer[curIdx] == (char)0)
                        {
                            return true;
                        }
                        if (buffer[curIdx] == '<' && buffer[curIdx + 1] == '/')
                        {
                            // 找到结点结束标签，栈顶结点出栈
                            curNode = stackNode.Pop();

                            curIdx += 2;
                            string nodeName = "";
                            curIdx = parseIdentifier(ref buffer, curIdx, out nodeName);
                            if (curNode.getName() != nodeName)
                            {
                                throw new Exception("结束标签名称不匹配");
                            }
                            if (buffer[curIdx] != '>')
                            {
                                throw new Exception("结束标签名称不匹配");
                            }
                            curIdx += 1;
                            curIdx = skipWhiteSpace(ref buffer, curIdx);
                            continue;
                        }
                    }

                    {
                        // 解析开始标志
                        curIdx = skipWhiteSpace(ref buffer, curIdx);
                        if (buffer[curIdx] == (char)0)
                        {
                            return true;
                        }
                        if (buffer[curIdx] != '<')
                        {
                            throw new Exception("找不到开始标签 <");
                        }
                        curIdx++;
                    }


                    {
                        // 过滤注释
                        int lastIdx = curIdx;

                        curIdx = skipComment(ref buffer, curIdx);

                        if (lastIdx != curIdx)
                        {
                            curIdx = skipWhiteSpace(ref buffer, curIdx);
                            continue;
                        }
                    }

                    {
                        // 解析节点名字
                        curIdx = skipWhiteSpace(ref buffer, curIdx);
                        if (newNode == null)
                        {
                            string nodeName = "";
                            curIdx = parseIdentifier(ref buffer, curIdx, out nodeName);
                            if (buffer[curIdx] == (char)0)
                            {
                                throw new Exception("无法解析节点名字");
                            }

                            curNode.setName(nodeName);
                            newNode = curNode;
                        }
                        else
                        {
                            string nodeName = "";
                            curIdx = parseIdentifier(ref buffer, curIdx, out nodeName);
                            if (buffer[curIdx] == (char)0)
                            {
                                throw new Exception("无法解析节点名字");
                            }

                            newNode = new MarkupNode(curNode);
                            newNode.setName(nodeName);
                            stackNode.Push(newNode);
                            curNode = newNode;
                        }
                    }

                    {
                        // 解析节点属性值
                        bool result = parseAttributes(ref buffer, ref curIdx, ref curNode);
                        if (result == false)
                        {
                            return false;
                        }

                        // 解析节点结束标签
                        curIdx = skipWhiteSpace(ref buffer, curIdx);
                        if (buffer[curIdx] == '/' && buffer[curIdx + 1] == '>')
                        {
                            curIdx += 2;
                            stackNode.Pop();
                        }
                        else
                        {
                            // 解析节点关闭标签
                            if (buffer[curIdx] != '>')
                            {
                                throw new Exception("找不到关闭标签 >");
                            }
                            {
                                // 解析节点值
                                curIdx++;
                                string value = "";
                                bool parseResult = parseData(ref buffer, ref curIdx, '<', out value);
                                if (parseResult == false)
                                {
                                    return false;
                                }
                                if (isWhiteSpace(value))
                                {
                                    value = "";
                                }
                                curNode.setValue(value);
                            }
                            {
                                if (buffer[curIdx] == (char)0)
                                {
                                    return true;
                                }
                                if (buffer[curIdx] != '<')
                                {
                                    throw new Exception("找不到子节点开始标签 <");
                                }
                            }
                        }
                    }
                }

                stackNode.Clear();
                stackNode = null;
            }

            return true;
        }
        public bool release()
        {
            if (mRootNode != null)
            {
                Stack<MarkupNode> stackNode = new Stack<MarkupNode>();
                stackNode.Push(mRootNode);
                {
                    while (stackNode.Count() > 0)
                    {
                        MarkupNode node = stackNode.Peek();
                        List<MarkupNode> listNode = node.getChildList();

                        if (listNode.Count() > 0)
                        {
                            stackNode.Push(listNode[0]);

                            continue;
                        }

                        MarkupNode nodeDiscard = stackNode.Pop();
                        if (stackNode.Count() > 0)
                        {
                            MarkupNode nodeParent = stackNode.Peek();
                            nodeParent.removeChild(0);
                        }
                        nodeDiscard.release();
                        nodeDiscard = null;
                    }
                }

                stackNode.Clear();
                stackNode = null;
                mRootNode = null;
            }

            return true;
        }

        public int skipWhiteSpace(ref char[] buffer, int idx)
        {
            int start = idx;
            while (buffer[start] > (char)0 && buffer[start] <= ' ')
            {
                start++;
            }

            return start;
        }

        public int skipComment(ref char[] buffer, int idx)
        {
            int start = idx;

            if (buffer[start] == '!' || buffer[start] == '?')
            {
                char ch = buffer[start];
                if (ch == '!')
                {
                    ch = '-';
                }
                while (buffer[start] != (char)0 && !(buffer[start] == ch && buffer[start + 1] == '>'))
                {
                    start++;
                }
                if (buffer[start] != (char)0)
                {
                    start += 2;
                }
            }

            return start;
        }
        public int parseIdentifier(ref char[] buffer, int idx, out string identifier)
        {
            int start = idx;
            string name = "";
            identifier = "";

            while (buffer[start] != (char)0 && (buffer[start] == '_' || buffer[start] == ':' || isLetterOrDigit(buffer[start])))
            {
                name += buffer[start];

                start++;
            }
            identifier = name;

            return start;
        }

        public bool isLetterOrDigit(char ch)
        {
            bool result = char.IsLetter(ch) | char.IsDigit(ch);

            return result;
        }

        public bool parseAttributes(ref char[] buffer, ref int idx, ref MarkupNode curNode)
        {
            int start = idx;

            if (buffer[start] == '>')
            {
                return true;
            }

            start++;
            start = skipWhiteSpace(ref buffer, start);
            while (buffer[start] != (char)0 && buffer[start] != '>' && buffer[start] != '/')
            {
                string attrName = "";
                start = parseIdentifier(ref buffer, start, out attrName);
                start = skipWhiteSpace(ref buffer, start);
                if (buffer[start] != '=')
                {
                    throw new Exception("解析属性错误");
                }
                start++;
                start = skipWhiteSpace(ref buffer, start);
                if (buffer[start] != '"')
                {
                    throw new Exception("解析属性错误");
                }
                start++;
                string value = "";
                if (parseData(ref buffer, ref start, '"', out value) == false)
                {
                    throw new Exception("解析属性错误");
                }
                if (buffer[start] == (char)0)
                {
                    throw new Exception("解析属性错误");
                }
                {
                    XMLAttribute attribute = new XMLAttribute(attrName, value);
                    curNode.getAttributeList().Add(attribute);
                }
                start++;
                start = skipWhiteSpace(ref buffer, start);
            }

            idx = start;

            return true;
        }

        public bool parseData(ref char[] buffer, ref int idx, char endFlag, out string value)
        {
            int start = idx;
            value = "";

            while (buffer[start] != (char)0 && buffer[start] != endFlag)
            {
                while (buffer[start] == '&')
                {
                    start++;
                    start = parseMetaChar(ref buffer, start, ref value);
                }
                if (buffer[start] == ' ')
                {
                    value += buffer[start];
                    start++;
                }
                else if (buffer[start] != endFlag)
                {
                    value += buffer[start];
                    start++;
                }
            }

            idx = start;

            return true;
        }

        public int parseMetaChar(ref char[] buffer, int idx, ref string value)
        {
            int start = idx;

            if (buffer[idx] == 'a' && buffer[idx + 1] == 'm' && buffer[idx + 2] == 'p' && buffer[idx + 3] == ';')
            {
                value += '&';
                start += 4;
            }
            else if (buffer[idx] == 'l' && buffer[idx + 1] == 't' && buffer[idx + 2] == ';')
            {
                value += '<';
                start += 3;
            }
            else if (buffer[idx] == 'g' && buffer[idx + 1] == 't' && buffer[idx + 2] == ';')
            {
                value += '>';
                start += 3;
            }
            else if (buffer[idx] == 'q' && buffer[idx + 1] == 'u' && buffer[idx + 2] == 'o' && buffer[idx + 3] == 't' && buffer[idx + 4] == ';')
            {
                value += '"';
                start += 5;
            }
            else if (buffer[idx] == 'a' && buffer[idx + 1] == 'p' && buffer[idx + 2] == 'o' && buffer[idx + 3] == 's' && buffer[idx + 4] == ';')
            {
                value += '\'';
                start += 5;
            }
            else
            {
                value += '&';
            }

            return start;
        }
        public bool isWhiteSpace(string value)
        {
            if (value.Trim() == "")
            {
                return true;
            }

            return false;
        }

        protected MarkupNode mRootNode;
    }
}
