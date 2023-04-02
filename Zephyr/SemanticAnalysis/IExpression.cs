// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Zephyr.SemanticAnalysis;

public interface IExpression
{
    bool IsStatement { get; }
    bool IsUsed { get; }
    bool ReturnsValue { get; }
    bool CanBeDropped { get; }

    void SetIsStatement(bool isStatement);
    void SetUsed(bool used);
}
