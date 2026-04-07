# ActuarialForge

> A .NET library for actuarial reserving, built with a focus on clarity, composability, and domain correctness.

---

## Overview

**ActuarialForge** provides a structured and extensible framework for actuarial reserving methods, including:

- Chain Ladder
- Additive Method
- Bornhuetter-Ferguson
- Residual analysis
- Validation utilities

The library is designed around **explicit domain modeling** and **strong typing**, ensuring that actuarial logic remains transparent, reproducible, and robust.

---

## Design Goals

- **Domain-driven**: Clear separation between model, methods, workflow, and validation
- **Composable**: Methods can be reused and combined
- **Safe by design**: Guards against invalid actuarial states (e.g. currency mismatches, zero denominators)
- **Extensible**: Easy to add new reserving methods or validation approaches

---

## Getting Started

### 1. Build a Triangle from Claim Histories

```csharp
var triangle = new TriangleWorkflowBuilder()
    .FromClaimHistories(claims)
    .WithTimeGranularity(ReservingTimeGranularity.Yearly)
    .UsingClaimDateBasis(ClaimDateBasis.AccidentDate)
    .Build();
```

---

### 2. Apply a Reserving Method

```csharp
var chainLadder = new ChainLadder(triangle);

var projection = chainLadder.ComputeProjection();
var ultimates = chainLadder.ComputeUltimates();
```

---

### 3. Compute Loss Ratios

```csharp
var lossRatios = chainLadder.ComputeUltimateLossRatios(premiums);
```

---

### 4. Use Bornhuetter-Ferguson

```csharp
var bf = new BornhuetterFerguson(
    triangle,
    chainLadder,
    premiums
);

var bfUltimates = bf.ComputeUltimates();
```

---

## Architecture

The library is structured into clearly separated modules:

### ActuarialForge.Reserving.Model
Core domain objects:
- Triangle representations
- Money & currency handling
- Period abstractions

---

### ActuarialForge.Reserving.Methods
Implementation of reserving methods:
- ChainLadder
- AdditiveMethod
- BornhuetterFerguson

Includes:
- Development patterns (multiplicative / additive)
- Run-off factors
- Variance estimation

---

### ActuarialForge.Reserving.Workflow
Transformation layer from raw data to triangles:
- ClaimHistory
- ClaimHistoryEvent
- TriangleWorkflowBuilder

---

### ActuarialForge.Reserving.Validation
Validation and diagnostics:
- Residual analysis
- Correlation structures
- Method comparison

---

### ActuarialForge.Utils
Mathematical utilities:
- Linear regression
- Decimal-based numerical helpers

---

## Example: End-to-End Workflow

```csharp
var triangle = new TriangleWorkflowBuilder()
    .FromClaimHistories(claims)
    .WithTimeGranularity(ReservingTimeGranularity.Yearly)
    .UsingClaimDateBasis(ClaimDateBasis.AccidentDate)
    .Build();

var method = new ChainLadder(triangle);

var projection = method.ComputeProjection();
var ultimates = method.ComputeUltimates();

var residuals = new Residuals(method);
var correlation = AccidentPeriodCorrelationCalculator.Compute(residuals);
```

---

## Key Concepts

### Development Patterns
- Multiplicative (Chain Ladder)
- Incremental / additive

### Run-off Factors
Encapsulated via IRunOffFactors, allowing:
- unified access
- method-specific interpretation

### Strong Typing
- AccidentPeriod
- DevelopmentPeriod
- Money
- Currency

---

## Validation Philosophy

The library enforces correctness early:

- Currency mismatches throw immediately
- Invalid triangles are rejected
- Division-by-zero situations are guarded explicitly

---

## Extending the Library

You can easily add:

- New reserving methods via IPatternBasedReservingMethod
- Custom validation logic via IReservingMethodSelector
- Alternative workflows or data sources

---

## Contributing

Contributions are welcome.  
Please ensure:

- Domain consistency is preserved
- XML documentation is included
- Edge cases are handled explicitly

---

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## Author

Jan Braunschmidt
