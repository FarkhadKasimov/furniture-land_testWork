// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("LyHj1VfE7qATj297qmWfBCIQzgKzMD4xAbMwOzOzMDAxsqhMciTavC0T6Rr5u/N2a8/zHfpww3wEJE3Cvrf+ymqRYAZghBZesqoHqjYjCGhP6yFjSYn0zIS78jZEVjY0PHgrp1taQ/OHdYkHPQIqygHQbyyJcKFJGPEtUVA/xm56RSLqZn8YBfnhhv9TWXUwj/Lp/tuRdElZOlxxbMFccNbPKCdyDi7OPDOTIq8K/rL4iLZyAbMwEwE8Nzgbt3m3xjwwMDA0MTJdMV5TGuZzxiuk+vlHmHUVmrj8qV8+DMusq6cVK6y5axMjgnn/g+xgtotVvEp0L2C0H33ikwOgtKlr/gmYrsYfqjsUeAJo+NfNxmrW31Y1fxd3HXorU+HayDMyMDEw");
        private static int[] order = new int[] { 8,1,11,5,6,12,11,13,9,9,10,13,12,13,14 };
        private static int key = 49;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
