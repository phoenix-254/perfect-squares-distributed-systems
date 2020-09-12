# perferct-squares

An interesting problem in arithmetic with deep implications to elliptic curve theory is the problem of finding perfect squares that are sums of consecutive squares. A classic example is the Pythagorean identity:

<img src="https://render.githubusercontent.com/render/math?math=3^2{%20%2B%20}4^2{%20=%20}5^2">

that reveals that the sum of squares of 3, 4 is itself a square. 

A more interesting example is Lucas' Square Pyramid:

<img src="https://render.githubusercontent.com/render/math?math=1^2{%20%2B%20}2^2{%20%2B%20}...{%20%2B%20}24^2{%20=%20}70^2">

In both of these examples, sums of squares of consecutive integers form the square of another integer.

The goal of this first project is to use F# and the actor model to build a good solution to this problem that runs well on multi-core machines.