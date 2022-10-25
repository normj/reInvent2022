using System.Text;

using Amazon;
using Amazon.KeyManagementService;
using AWS.EncryptionSDK;
using AWS.EncryptionSDK.Core;

namespace SQSBatchExample.Common;

public interface IMessageEncryption
{
    string Encrypt(string plainTextMessage);

    string Decrypt(string encryptedMessage);
}

public class MessageEncryption : IMessageEncryption
{
    public const string MESSAGE_KMS_KEY_ENV = "MESSAGE_KMS_KEY";

    IAwsEncryptionSdk _encryptionSdk;
    IKeyring _keyRing;

    public MessageEncryption()
    {
        var kmsKeyArn = Environment.GetEnvironmentVariable(MESSAGE_KMS_KEY_ENV);
        if(string.IsNullOrEmpty(kmsKeyArn))
        {
            throw new ArgumentNullException($"Environment variable {MESSAGE_KMS_KEY_ENV} is not set for the KMS key to configure for encryption");
        }

        SetupEncryptionSdk(kmsKeyArn);
    }

    public MessageEncryption(string kmsKeyArn)
    {
        SetupEncryptionSdk(kmsKeyArn);
    }

    private void SetupEncryptionSdk(string kmsKeyArn)
    {
        var kmsKeyRegion = RegionEndpoint.GetBySystemName(Arn.Parse(kmsKeyArn).Region);

        this._encryptionSdk = AwsEncryptionSdkFactory.CreateDefaultAwsEncryptionSdk();

        var materialProviders = AwsCryptographicMaterialProvidersFactory.CreateDefaultAwsCryptographicMaterialProviders();

        var kmsKeyringInput = new CreateAwsKmsKeyringInput
        {
            KmsClient = new AmazonKeyManagementServiceClient(kmsKeyRegion),
            KmsKeyId = kmsKeyArn
        };

        this._keyRing = materialProviders.CreateAwsKmsKeyring(kmsKeyringInput);
    }

    public string Encrypt(string plainTextMessage)
    {            
        var encryptInput = new EncryptInput
        {
            Plaintext = new MemoryStream(UTF8Encoding.UTF8.GetBytes(plainTextMessage)),
            Keyring = this._keyRing
        };

        var encryptOutput = this._encryptionSdk.Encrypt(encryptInput);
        var encryptedText = Convert.ToBase64String(encryptOutput.Ciphertext.ToArray());
        return encryptedText;
    }

    public string Decrypt(string encryptedMessage)
    {
        var decryptInput = new DecryptInput
        {
            Ciphertext = new MemoryStream(Convert.FromBase64String(encryptedMessage)),
            Keyring = this._keyRing
        };

        var decryptOuptut = this._encryptionSdk.Decrypt(decryptInput);
        var decryptedText = UTF8Encoding.UTF8.GetString(decryptOuptut.Plaintext.ToArray());

        return decryptedText;
    }
}
