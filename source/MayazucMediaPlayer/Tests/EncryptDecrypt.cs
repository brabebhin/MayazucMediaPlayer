//using MCMediaCenterShared.Networking.Security;
//using System;
//using System.Collections.Generic;
//using System.Runtime.InteropServices.WindowsRuntime;
//using System.Text;
//using System.Threading.Tasks;

//namespace MCMediaCenterShared.Tests
//{
//    class EncryptionTests
//    {
//        internal async Task
//            EncryptDecrypt()
//        {
//            AESSecurityProvider asymetricProvider = new AESSecurityProvider();
//            var publicKey = asymetricProvider.PublicKey;

//            SymSecurityProvider symetricProvider = new SymSecurityProvider();
//            var encryptedSymkey = symetricProvider.EncryptKey(publicKey);

//            var messageToEncrypt = "wekjfwhekuchkuebckjshabca,kjsncoiqwhdqwo8daucbajsnx ma scn.abvchagclualhwyu98687567475rpoj'kpl'lw;dnmqkwancdskvhnsifhsiefuhyseudcbnsmd, v.nsdvjbsdhgwyeuifge7o8ftwe7f5w69dr6wefgcshjdvbclsdkjvcn;sdivjsdpojvposjde9peufp;ivnkldvknn'knk'nk'nk'nk'nk'nhuugfyftd54s4efgh jbhnkm,";

//            //protocol.

//            var bytes = Encoding.UTF8.GetBytes(messageToEncrypt);

//            var bufferToEncrypt = bytes.AsBuffer();
//            var encryptedBuffer = symetricProvider.EncryptData(bufferToEncrypt);

//            var decryptedKeyMaterial = await asymetricProvider.DecryptBuffer(encryptedSymkey);
//            SymSecurityProvider decrytptor = new SymSecurityProvider();
//            decrytptor.LoadKey(decryptedKeyMaterial);

//            var decryptedMessage = await decrytptor.DecryptBufferAsync(encryptedBuffer);
//            var decrtyptedMessageString = Encoding.UTF8.GetString(decryptedMessage.ToArray());

//            if (messageToEncrypt != decrtyptedMessageString) throw new InvalidOperationException("Test failed");


//        }
//    }
//}
