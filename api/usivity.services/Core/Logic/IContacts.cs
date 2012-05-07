using MindTouch.Xml;
using Usivity.Entities;

namespace Usivity.Services.Core.Logic {

    public interface IContacts {

        //--- Methods ---
        Contact GetContact(string id);
        Contact GetNewContact(XDoc info);
        XDoc GetContactXml(Contact contact, string relation = null);
        XDoc GetContactVerboseXml(Contact contact, string relation = null);
        XDoc GetContactsXml();
        void UpdateContactInformation(Contact contact, XDoc info);
        void SaveContact(Contact contact);
        void RemoveContact(Contact contact);
    }
}
