# perfect-squares

An interesting problem in arithmetic with deep implications to elliptic curve theory is the problem of finding perfect squares that are sums of consecutive squares. A classic example is the Pythagorean identity:

<img src="https://render.githubusercontent.com/render/math?math=3^2{%20%2B%20}4^2{%20=%20}5^2">

that reveals that the sum of squares of 3, 4 is itself a square. 

A more interesting example is Lucas' Square Pyramid:

<img src="https://render.githubusercontent.com/render/math?math=1^2{%20%2B%20}2^2{%20%2B%20}...{%20%2B%20}24^2{%20=%20}70^2">

In both of these examples, sums of squares of consecutive integers form the square of another integer.

The goal of this project is to use F# and the actor model to build a good solution to this problem that runs well on multi-core machines.

### Instruction to run
--- 
1. This project has been developed using Visual Studio Code and ionide extension [ref](https://docs.microsoft.com/en-us/dotnet/fsharp/get-started/get-started-vscode). Also make sure you have dotnet sdk installed.
2. Program.fsx file contains the main logic for the project. To run the program locally use the command:<br/>
`dotnet fsi --langversion:preview Program.fsx 40 24`<br/>
Here, `--langversion:preview` is needed when I am writing this, may not be needed in future.
3. To run the program remotely use the command:<br/>
`dotnet fsi --langversion:preview Program.fsx 40 24 true`<br/>
Here, `true` is the argument tells the program that it should use remote actors.

### Result
---
I have achieved parallelism of around 7-8 times by using Akka actor model in this project. Some examples that I tested it on are as follows. [See screenshots]
<br/><br/>
Example: 1<br/>
- Input: <br/> N = 10^10, K = 2<br/>
- Output: <br/>
3, 20, 119, 696, 4059, 23660, 137903, 803760, 4684659, 27304196, 159140519, 9113711091, 9174492471, 9201170207, 9203501348, 4228898943, 4294967295, 9361954743, 9417254376, 9482878311, 3427722904, 3463683303, 927538920, 6134308447<br/>
- Run time: <br/>
real    0m38.907s<br/>
user    4m27.674s<br/>
sys     0m0.514s<br/>
CPU time to Real time ratio : 6.8<br/>

Example: 2<br/>
- Input: <br/> N = 10^11, K = 2<br/>
- Output: <br/>
3, 20, 119, 696, 4059, 23660, 137903, 803760, 4684659, 27304196, 159140519, 927538920, 75957200803, 76058624475, 51027785007, 76105188675, 38657389187, 14397045560, 39578320631, 77428752055, 64963961119, 15295287404, 3427722904, 3463683303, 78435184395, 78907549580, 16443921616, 4228898943, 4294967295, 91615519499, 29455579727, 67865704712, 68182518911, 6134308447, 44168195899, 69867701407, 95317973280, 82885205951, 82942587616, 45660904536, 33439886731, 20617974291, 96089133283, 71281441355, 9113711091, 21268305827, 9174492471, 9201170207, 9203501348, 71613130771, 9361954743, 9417254376, 9482878311, 84163829912, 84186789032, 96718872692, 97184579827, 97190222672, 97190607696, 97194169356, 97209046447, 97279817676, 84882215608, 72893482071, 85323105696, 48024363619, 85630496859, 61271285392, 61434523891, 48877375123, 86279592264, 12270139684, 12271335131, 24350538803, 24372902768, 99459260204, 87056560363<br/>
- Run time: <br/>
real    6m2.195s<br/>
user    47m1.980s<br/>
sys     0m0.697s<br/>
CPU time to Real time ratio : 7.8
