using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace BrunoMLopes.CullWindows {
    public interface IOptionsProvider {
        int NumberOfOpenFiles { get; }
    }

    public class DocumentMonitor : IVsRunningDocTableEvents, IDisposable
    {
        private readonly IServiceProvider _sp;
        private readonly IOptionsProvider _optionsProvider;
        private List<Document> _documentList;
        private IVsRunningDocumentTable _rdt;
        private uint _rdtCookie;

        public int NumberOfOpenTabs {
            get { return _optionsProvider.NumberOfOpenFiles; }
        }

        public DocumentMonitor(IServiceProvider sp, IOptionsProvider optionsProvider) {
            this._sp = sp;
            this._optionsProvider = optionsProvider;
            this._documentList = new List<Document>();

            _rdt = (IVsRunningDocumentTable)sp.GetService(typeof(SVsRunningDocumentTable));
            if(_rdt == null) return;
            if(_rdt.AdviseRunningDocTableEvents(this, out _rdtCookie) != VSConstants.S_OK) {
                debugMessage("failed to advice RTD");
            }
        }

        #region Implementation of IVsRunningDocTableEvents

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            showEvent(new Document(getNameForDocument(docCookie), DateTime.Now));
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            hideEvent(new Document(getNameForDocument(docCookie), DateTime.Now));
            return VSConstants.S_OK;
        }

        #endregion

        private void showEvent(Document document)
        {
            var doc = _documentList.SingleOrDefault(d => d.Filepath == document.Filepath);
            
            if (doc == null)
            {
                _documentList.Add(document);
            }
            else
            {
                doc.LastAccess = document.LastAccess;
            }

            _documentList.Sort((a, b) => b.LastAccess.CompareTo(a.LastAccess));
            int numberOfTries = _documentList.Count - NumberOfOpenTabs;
            while (numberOfTries > 0)
            {
                var last = _documentList.Last();
                if (closeDocument(last))
                {
                    _documentList.Remove(last);
                }
                numberOfTries -= 1;
            }
            
        }

        private void hideEvent(Document document)
        {
            debugMessage(string.Format("hide doc {0}", document.Filepath));
            _documentList.Remove(document);
        }

        private bool closeDocument(Document document)
        {
            object property;
            IVsWindowFrame windowFrame;
            uint itemId;
            IVsUIHierarchy hierarchy;
            if(!VsShellUtilities.IsDocumentOpen(_sp, document.Filepath, Guid.Empty, out hierarchy, out itemId, out windowFrame)) {
                return false;
            }

            windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out property);
            var textBuffer = property as IVsTextBuffer;

            if (textBuffer == null) return false;

            uint bufferStatus;
            textBuffer.GetStateFlags(out bufferStatus);
            if ((bufferStatus & (uint) BUFFERSTATEFLAGS.BSF_MODIFIED) != 0) return false;

            windowFrame.CloseFrame((uint) __FRAMECLOSE.FRAMECLOSE_PromptSave);
            return true;
        }

        string getNameForDocument(uint docCookie) {
                        IntPtr docData;
            uint itemId;
            IVsHierarchy hierarchy;
            uint rdtFlags;
            uint readLocks;
            uint editLocks;
            string mkDocument;

            if(_rdt.GetDocumentInfo(docCookie, out rdtFlags, out readLocks, out editLocks, out mkDocument,
                                 out hierarchy, out itemId, out docData) != VSConstants.S_OK) {
                return null;
            }
            return mkDocument;
        }

        private void debugMessage(string s) {
            VsShellUtilities.GetOutputWindowPane(_sp, VSConstants.GUID_OutWindowGeneralPane).OutputString(s+Environment.NewLine);
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_rdtCookie != 0) _rdt.UnadviseRunningDocTableEvents(_rdtCookie);
        }

        #endregion
    }

    internal class Document : IEquatable<Document>
    {
        private readonly string moniker;

        public DateTime LastAccess { get; set; }
        public string Filepath
        {
            get
            {
                return moniker;
            }
        }
        public Document(string moniker) : this(moniker, DateTime.Now)
        {
        }
        public Document(string moniker, DateTime dateTime) {
            this.moniker = moniker;
            this.LastAccess = dateTime;
        }


        public bool Equals(Document other) {
            return moniker == other.moniker;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Document)) return false;
            return Equals((Document)obj);
        }

        public override int GetHashCode()
        {
            return moniker.GetHashCode();
        }

        public static bool operator ==(Document left, Document right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Document left, Document right)
        {
            return !Equals(left, right);
        }
    }
}