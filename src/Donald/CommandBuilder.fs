﻿[<AutoOpen>]
module Donald.CommandBuilder

open System
open System.Data

type CommandSpec<'a> = 
    {
        Connection     : IDbConnection
        Transaction    : IDbTransaction option
        CommandType    : CommandType
        CommandTimeout : int option
        Statement      : string 
        Param          : RawDbParams
    }
    static member Create (conn : IDbConnection) = 
        {
            Connection     = conn
            Transaction    = None
            CommandType    = CommandType.Text
            CommandTimeout = None
            Statement      = ""
            Param          = []
        }

type CommandBuilder<'a>(conn : IDbConnection) =
    member _.Yield(_) = CommandSpec<'a>.Create (conn)

    member _.Run(spec : CommandSpec<'a>) = 
        let param = DbParams.create spec.Param
        match spec.Transaction with 
        | Some tran -> 
            tran.NewCommand(spec.CommandType, spec.Statement, spec.CommandTimeout)
                .SetDbParams(param)
        
        | None ->
            spec.Connection
                .NewCommand(spec.CommandType, spec.Statement, spec.CommandTimeout)
                .SetDbParams(param)

    [<CustomOperation("cmdParam")>]
    member _.DbParams (spec : CommandSpec<'a>, param : RawDbParams) =
        { spec with Param = param }

    [<CustomOperation("cmdText")>]
    member _.Statement (spec : CommandSpec<'a>, statement : string) =
        { spec with Statement = statement }

    [<CustomOperation("cmdTran")>]
    member _.Transaction (spec : CommandSpec<'a>, tran : IDbTransaction) =
        { spec with Transaction = Some tran }
    
    [<CustomOperation("cmdType")>]
    member _.CommandType (spec : CommandSpec<'a>, commandType : CommandType) =
        { spec with CommandType = commandType }
    
    [<CustomOperation("cmdTimeout")>]
    member _.CommandTimeout (spec : CommandSpec<'a>, timeout : TimeSpan) =
        { spec with CommandTimeout = Some <| int timeout.TotalSeconds }

let dbCommand<'a> (conn : IDbConnection) = CommandBuilder<'a>(conn)