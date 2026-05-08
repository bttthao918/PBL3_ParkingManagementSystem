# Swagger Test Data

## Login Accounts

Use `POST /api/auth/login`.

| Role | Email | Password | Related data |
| --- | --- | --- | --- |
| Manager | `th04092006@gmail.com` | `Huong@4906` | `MGR001` |
| Employee | `thanh76555765@gmail.com` | `Huong4906@` | `EMP001`, shift `Sáng` |
| Employee | `staff.hung@gmail.com` | `Huong4906@` | `EMP002`, shift `Chiều` |
| Disabled employee | `staff.disabled@gmail.com` | `Huong4906@` | `EMP003`, inactive account |
| Customer | `th04092006.customer@gmail.com` | `Huong4906@` | `CUS001` |
| Customer | `customer.lan@gmail.com` | `Huong4906@` | `CUS002` |
| Customer | `customer.khoa@gmail.com` | `Huong4906@` | `CUS003` |
| Customer | `customer.huong@gmail.com` | `Huong4906@` | `CUS004` |
| Customer | `customer.minh@gmail.com` | `Huong4906@` | `CUS005` |

## Employee Invite

Use these for invite-related endpoints:

| Case | Token | Email | Notes |
| --- | --- | --- | --- |
| Pending invite | `INVITE-EMP004-2026` | `staff.invited@gmail.com` | Valid until `2030-12-31` |
| Used invite | `INVITE-USED-EMP005` | `staff.usedinvite@gmail.com` | Already used |

Note: the current `ConfirmInviteAsync` code only marks the invite as used. The account activation/create-account behavior still needs a service fix.

Confirm invite sample body:

```json
{
  "inviteToken": "INVITE-EMP004-2026",
  "fullName": "Ngô Minh An",
  "phoneNumber": "0977000111",
  "password": "Huong4906@",
  "confirmPassword": "Huong4906@"
}
```

## IDs For Testing

Parking slots:

`A01`, `A05`, `B01`, `C01` are `Đang sử dụng`.
`A02`, `A03`, `B02`, `B03`, `C02` are `Trống`.
`A04` is `Đã đặt`.
`A06`, `C03` are `Bảo trì`.

Tickets:

`TKT001`, `TKT002`, `TKT003`, `TKT004` are active check-ins.
`TKT005`, `TKT006`, `TKT007`, `TKT008` are checked out and paid.

Monthly tickets:

`MTK001`, `MTK002` are active.
`MTK003` is expired.
`MTK004` is cancelled.

OTP samples:

`customer.pending@gmail.com` has OTP `123456`.
`th04092006.customer@gmail.com` has verified OTP `654321`.

## Registration Test

For new customer registration, use a new Gmail address that is not in the table above, for example:

```json
{
  "email": "customer.newtest@gmail.com",
  "password": "Huong4906@",
  "confirmPassword": "Huong4906@",
  "fullName": "Người Dùng Test",
  "phoneNumber": "0988123456"
}
```
