[![CC0](http://i.creativecommons.org/p/zero/1.0/88x31.png)](http://creativecommons.org/publicdomain/zero/1.0/)  
To the extent possible under law, [SplittyDev](https://github.com/SplittyDev) has waived all copyright and related or neighboring rights to Hashlecter.

![HashLecter Logo](http://i.imgur.com/Wt4bxbY.png)  
Free (as in speech) md5 hash collider.

## Using Hashlecter
### Command-line arguments
| Short | Long       | Type     | Description
|-------|------------|----------|-------------
| -i    | --input    | Argument | Read hashes from file
| -d    | --dict     | Argument | Perform a dictionary-attack using the specified file
| -s    | --session  | Argument | Specify a session name
| -r    | --rounds   | Argument | Specify hashing rounds
|       | --stdin    | Switch   | Read hashes from stdin
|       | --silent   | Switch   | Don't output anything to stdout
|       | --wizard   | Switch   | Easy configuration
|       | --show     | Switch   | Show results; can be combined with `-s/--session`

Info:  
The default hashing method is md5.

Important:  
The `-r/--rounds` option only works with the base hashes (md5, sha1, sha256).

#### Experimental/Unstable features
| Short | Long            | Type     | Description
|-------|-----------------|----------|-------------
|       | --exp-lazy-eval | Switch   | Lazy evaluation of input dictionary

### Hashing methods
* . md5
* . md5_double
* ! md5_salted
* ! md5_mybb
* . sha1
* . sha1_double
* . sha256
* . sha256_double

Hashing methods marked with a period use the following format:
```
# This is a comment. It will be ignored.
# Hash 1
6ae99d4d2de5e3cbd29fec87ae7d76eb
# Hash 2
50a39ec9e0e46cf2826eb5745e1c800b
```

Hashing methods marked with an exclamation point use the following format:
```
# This is a comment. It will be ignored.
# Hash 1 : Salt 1
6ae99d4d2de5e3cbd29fec87ae7d76eb:91704826
# Hash 2 : Salt 2
50a39ec9e0e46cf2826eb5745e1c800b:72946193
```

### Examples (mono)
Cracking the md5 hash for "hello" using dictionary "dict":  
```
# mono lecter.exe -d dict --stdin | echo 5d41402abc4b2a76b9719d911017c592
```

Cracking a list "hashes" of 2-round md5-hashed entries using dictionary "dict":
```
# mono lecter.exe -d dict -i hashes -m md5_double
```

### Examples (Microsoft .Net)
Cracking the md5 hash for "hello" using dictionary "dict":
```
> lecter.exe -d dict --stdin | echo 5d41402abc4b2a76b9719d911017c592
```

Cracking a list "hashes" of 2-round md5-hashed entries using dictionary "dict":
```
> lecter -d dict -i hashes -m md5_double
```

## Building Hashlecter
### Building under *nix (mono)
```
# git clone https://github.com/SplittyDev/Hashlecter.git hashlecter
# cd hashlecter
# xbuild /p:Configuration=Release
```

### Building under Windows
```
> git clone https://github.com/SplittyDev/Hashlecter.git hashlecter
> cd hashlecter
> msbuild /p:Configuration=Release
```

### Building using Visual Studio / MonoDevelop
Just open the solution file and build the project in Release mode
