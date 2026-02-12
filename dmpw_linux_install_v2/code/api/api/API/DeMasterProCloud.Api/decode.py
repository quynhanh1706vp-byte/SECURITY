from Crypto.Cipher import AES
from Crypto.Util import Counter
from Crypto import Random
import base64
import pkcs7

# Example key : 9t4RiWy2+s6/dJzbixh3RQ==

def decrypt(key, ciphertext):
    # Initialize counter for decryption. iv should be the same as the output of
    # encrypt().
    ivAscii = key
    # Create AES-CTR cipher.
    aes = AES.new(key, AES.MODE_CBC, base64.b64decode(ivAscii))

    # Decrypt and return the plaintext.
    plaintext = aes.decrypt(ciphertext)
    return plaintext


if __name__ == '__main__':
    secret_key = str(raw_input("Enter your key : "))
    decode_text = str(raw_input("Input text you want to decode:  "))
    decode_text = decrypt(base64.b64decode(secret_key), base64.b64decode(decode_text))
    print(decode_text)
