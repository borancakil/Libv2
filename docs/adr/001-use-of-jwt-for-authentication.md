# ADR-001: Use of JSON Web Tokens (JWT) for Authentication

**Date**: 2025-08-22

**Status**: Accepted

## Context

We need a secure and efficient mechanism for authenticating users in our stateless RESTful API. The chosen mechanism should support role-based access control and be compatible with modern web and mobile clients.

## Decision

We have chosen to use JSON Web Tokens (JWT) for authentication. Specifically, we will use a nested JWS/JWE scheme, where the token is both signed (JWS) for integrity and encrypted (JWE) for confidentiality. This provides a high level of security for the token's contents.

## Consequences

### Advantages

*   **Stateless**: JWTs are self-contained and do not require the server to maintain session state.
*   **Secure**: The use of JWS and JWE ensures the integrity and confidentiality of the token.
*   **Flexible**: JWTs can be used with a variety of clients, including web browsers and mobile apps.
*   **Scalable**: The stateless nature of JWTs makes it easy to scale the application horizontally.

### Disadvantages

*   **Token Size**: Encrypted JWTs can be larger than traditional session cookies.
*   **Revocation**: Token revocation is more complex than with stateful sessions. We will implement a short access token lifetime and a refresh token mechanism to mitigate this.
