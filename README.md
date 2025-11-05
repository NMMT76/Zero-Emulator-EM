Fork of ArjunNair's Zero Emulator, trimmed down to ZX Spectrum 48K only and with added EdgeMaster "virtual expansion device" and ULA-SX (simple expansion) support.

A potential implementation of the EdgeMaster in real hardware would/should always be based about some form of microcontroller, as that allows the simplest form of "expansion through code"

The whole idea behind the EdgeMaster, so named because it does NOT do DMA/busmastering, was that, given a "slow enough" platform, even a simple PIO device implementing a very basic form of RPC could:
1. provide infinite expansion/functionality
2. provide some form of acceleration
bounded only by:
- the IO speed
- the external "coprocessor" speed

While 1 is quite self evident, 2 is a bit more complicated. For it to be true, the time of execution for the "original code" needs to be higher than the time it takes to push data to the EM, do whatever work needs to be done and get the results back from the EM.

(all results from here on are based on Zero running on a R5 5600, YMMV)

One of the "weakest" part of the ZX is floating point operations. As show in EM_Tests_MathBasic, delegating the operations to the EM, even the basic math floating point functions will be "slightly faster" to "4x faster".
When you go to trigonometric functions, as in EM_Tests_BENCH01, the speed gains become very evident, in two different ways. One, the EM is so fast that is spends most of the time waiting for the ZX to fetch the results, the ZX is never waiting on the EM to finish. Two, because of the former, from the ZX point of view, the time the EM takes for a simple operation is no different from the time the EM takes to perform a not so simple operation. EM_Tests_BENCH02 and EM_Tests_BENCH03 perfectly illustrate this.
