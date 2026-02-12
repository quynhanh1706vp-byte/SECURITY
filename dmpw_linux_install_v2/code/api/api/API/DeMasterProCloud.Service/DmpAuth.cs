//using System;
using System.Runtime.InteropServices;

namespace DeMasterProCloud.Service
{
    public class DmpAuth
    {
        [DllImport("Dmp_Api")]
        public static extern char DMP_auth_makekey(byte[] pRID, byte[] pTimeStamp);

        [DllImport("Dmp_Api")]
        public static extern char DMP_auth_step2(byte[] pRID, byte[] pTimeStamp, byte[] pRecvData, int nRecvLen, [In,Out] byte[] pRespData, out int nRespLen);

        [DllImport("Dmp_Api")]
        public static extern char DMP_auth_step3_validcheck(byte[] pRID, byte[] pRecvData, int nRecvLen);

        [DllImport("Dmp_Api")]
        public static extern char DMP_encryptCertData(byte[] pRID, [In, Out] byte[] pSrc, int nSrc, [In, Out] byte[] pDst, out int nDst);
    }
}
