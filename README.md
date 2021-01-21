## What is this API?
This is an API for Encryption Module (see my other repositories), which handles device functions using C# SerialPort API. It's written in .NET Core.

## How to use it?
Just create `EncryptionModule` class and use it's methods. Example:
```cs
var sequence = "MyDataToEncrypt";

var module = new EncryptionModule();
module.SetPassword("test").InitializeCipher();
var outData = module.EncryptSequence(sequence);

// outData is a byte array that consists of encrypted data.
```

## What type of data can it encrypt?
Both `byte[]` and `string` are supported, so you can encrypt any data you wish.

## What encoding does it use for string?
It uses ASCII.

## Do I need to call `InitializeCipher()` once?
No, you need to call that method every single time you start encrypting new data.
The algorithm itself supports stream encryption, so when you would not initialize the algorithm it would think that data has not ended yet and would encrypt it in same sequence.

Example:
`Initialize, Encrypt Sequence (test), Encrypt Sequence (test)` is same as `Initialize, Encrypt Sequence (testtest)`.
To start new encryption (eg. you encrypt another file) you need to call `InitializeCipher()` again.

## Which commands does it support?
It supports most of the EMO commands excluding 0x6 (stream encryption) that is mostly useless (it exists, but in most cases using sequence is better, because you do not lock device in encrypting state, unless you will have large overhead based on sending sequence lengths, so 0x6 is useful in really rare cases).

Commands supported: 
- `0x04` - set password
- `0x05` - init encryption algorithm
- `0x07` - encrypt sequence
- `0x50` - dump encryption data
- `0x51` - load encryption data

## How about dumping encryption data?
Yes, you can dump VMPC algorithm values (after initialization). It is recommended way to encrypt large files on your PC's CPU instead on EMO. To do this you need to call `module.DumpEncryptionData()` method.

It will dump (in order): `P[]` - permutation table of VMPC, `s` - s-value of VMPC, `n` - n-counter of VMPC.

## How to load encryption data?
That's harder. You need to call `module.LoadEncryptionData(byte[] array)` where array is equal to 258 bytes. First 256 bytes are `P[]` (permutation table) of VMPC algorithm, two next bytes are `s` and `n` values from VMPC algorithm. **ORDER MATTERS!**
