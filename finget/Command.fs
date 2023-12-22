namespace finget

open System.Diagnostics

type Executable = string
type Argument = string
type Arguments = Argument list
type Command = Executable * Arguments

type ExitCode = int
type StdOut = string
type StdErr = string
type ExecutionResult = ExitCode * StdOut * StdErr

module Command =

    let run (command: Command) : ExecutionResult =
        let executable, arguments = command

        use childProcess =
            new Process(
                StartInfo =
                    ProcessStartInfo(
                        executable,
                        arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    )
            )

        childProcess.Start() |> ignore
        let output = childProcess.StandardOutput.ReadToEnd()
        let error = childProcess.StandardError.ReadToEnd()

        childProcess.WaitForExit()

        childProcess.ExitCode, output, error
