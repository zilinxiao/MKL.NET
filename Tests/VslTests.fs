﻿module VslTests

open MKLNET
open CsCheck

let all =
    test "Vsl" {

        test "stream" {
            let stream = Vsl.NewStream(VslBrng.MRG32K3A, 77u)
            let r = Array.zeroCreate 3
            Vsl.RngGaussian(VslMethodGaussian.ICDF, stream, r.Length, r, 0.0, 1.0) |> Check.equal 0
            Vsl.DeleteStream stream |> Check.equal 0
            let expected = [| 2.197108196; 0.1405333963; -0.4023745985 |]
            Check.close VeryHigh expected r
        }

        test "mean" {
            let! obvs = Gen.Int.[1,100]
            let! vars = Gen.Int.[1,100]
            let! x = Gen.Double.[1.0,2.0].Array.[obvs*vars]
            let mean = Array.zeroCreate<float> vars
            let task = Vsl.SSNewTask(vars, obvs, VslStorage.ROWS, x)
            Vsl.SSEditTask(task, VslEdit.MEAN, mean) |> Check.equal 0
            Vsl.SSCompute(task, VslEstimate.MEAN, VslMethod.FAST) |> Check.equal 0
            Vsl.SSDeleteTask task |> Check.equal 0
            let expected = Array.init vars (fun i ->
                let mutable total = 0.0
                for j = 0 to obvs - 1 do
                    total <- total + x.[i * obvs + j]
                total / float obvs
            )
            Check.close VeryHigh expected mean
        }

        test "mean_weight" {
            let! obvs = Gen.Int.[1,100]
            let! vars = Gen.Int.[1,100]
            let! x = Gen.Double.[1.0,2.0].Array.[obvs*vars]
            let mean = Array.zeroCreate<float> vars
            let weight = Array.init obvs (fun i -> float(i+1))
            let task = Vsl.SSNewTask(vars, obvs, VslStorage.ROWS, x, weight)
            Vsl.SSEditTask(task, VslEdit.MEAN, mean) |> Check.equal 0
            Vsl.SSCompute(task, VslEstimate.MEAN, VslMethod.FAST) |> Check.equal 0
            Vsl.SSDeleteTask task |> Check.equal 0
            let expected = Array.init vars (fun i ->
                let mutable total = 0.0
                let mutable totalWeight = 0.0
                for j = 0 to obvs - 1 do
                    let w = weight.[j]
                    totalWeight <- totalWeight + w
                    total <- total + x.[i * obvs + j] * w
                total / totalWeight
            )
            Check.close VeryHigh expected mean
        }
    }