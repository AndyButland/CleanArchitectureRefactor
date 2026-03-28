# Clean Architecture – Robert C. Martin (Summary)

## Core Thesis

The central argument is that good software architecture maximises the number of decisions *not yet made* — it keeps options open as long as possible, deferring commitments to frameworks, databases, UI, and infrastructure until you have enough information to decide wisely.

---

## Part 1 – Introduction: What is Design and Architecture?

Martin opens by challenging the idea that "architecture" and "design" are different things — they exist on a single continuum. The goal of both is to minimise the human effort required to build and maintain a system. He argues that the "mess" most codebases become is not inevitable; it's a failure of discipline.

---

## Part 2 – Programming Paradigms

Martin surveys the three major paradigms and what each *removes* from the programmer:

- **Structured programming** – removes unrestricted `goto` (introduces sequence, selection, iteration)
- **Object-oriented programming** – removes unrestricted use of function pointers (introduces polymorphism safely)
- **Functional programming** – removes assignment (immutability)

The key insight: OOP's most valuable contribution is **dependency inversion via polymorphism**, not encapsulation or inheritance.

---

## Part 3 – Design Principles (SOLID)

- **SRP** – Single Responsibility Principle: a module should have one, and only one, *reason to change* (i.e. one actor it serves)
- **OCP** – Open/Closed Principle: components should be open for extension, closed for modification
- **LSP** – Liskov Substitution Principle: subtypes must be substitutable for their base types without breaking behaviour
- **ISP** – Interface Segregation Principle: don't depend on things you don't use
- **DIP** – Dependency Inversion Principle: high-level policy should not depend on low-level detail; both should depend on abstractions

---

## Part 4 – Component Principles

**Cohesion** (what goes into a component):
- **REP** – Reuse/Release Equivalence: the unit of reuse is the unit of release
- **CCP** – Common Closure Principle: gather things that change together; the component-level SRP
- **CRP** – Common Reuse Principle: don't force users to depend on things they don't need

**Coupling** (how components relate):
- **ADP** – Acyclic Dependencies Principle: no cycles in the component dependency graph
- **SDP** – Stable Dependencies Principle: depend in the direction of stability
- **SAP** – Stable Abstractions Principle: stable components should be abstract; unstable ones can be concrete

---

## Part 5 – Architecture

### The Clean Architecture Diagram

The famous concentric-circles diagram organises code into layers, with the **Dependency Rule** as the governing law: *source code dependencies must point inward only*.

| Layer (outer → inner) | Contents |
|---|---|
| Frameworks & Drivers | Web, DB, UI, devices |
| Interface Adapters | Controllers, presenters, gateways |
| Application Business Rules | Use cases |
| Enterprise Business Rules | Entities |

### Key ideas in this section

- **Entities** encapsulate enterprise-wide, critical business rules. They are the most stable and the least likely to change.
- **Use Cases** contain application-specific business rules. They orchestrate the flow of data to and from entities.
- **The web is a detail.** So is the database. So is any framework. None of these belong at the centre of your architecture.
- **Boundaries** are drawn where change rates differ. Crossing a boundary always involves a source code dependency pointing inward.
- **Humble Objects** pattern: split testable logic from hard-to-test code (e.g. UI, I/O) at every boundary. The "humble" side does very little.

---

## Part 6 – Details

Martin drives home the point about specific "details":

- **The Database is a detail** — your business rules shouldn't know or care whether you use SQL Server, MongoDB, or flat files
- **The Web is a detail** — HTTP is a delivery mechanism, not an architectural cornerstone
- **Frameworks are details** — don't let a framework own your architecture; treat it as a plugin

---

## Overarching Themes

1. **Dependency management is everything.** Most architectural problems are dependency problems in disguise.
2. **Separate policy from detail.** High-level policy (business rules) must be insulated from low-level detail (I/O, frameworks, UI).
3. **Testability as a proxy for good design.** If something is hard to test, it's probably coupled to the wrong things.
4. **Defer irreversible decisions.** A good architecture lets you postpone choosing a database, a framework, or a communication protocol — and change those choices cheaply later.

---

It's a fairly opinionated book, and Martin is aware of that — some teams find the strict layering adds boilerplate for smaller systems. But the underlying principles (especially DIP, boundary thinking, and the separation of policy from detail) are widely influential and hold up well across scales.