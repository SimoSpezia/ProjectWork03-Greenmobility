export interface User {
    Id:string;
    Name: string;
    Surname: string;
    Email: string;
    Role : string;
};

export type CreateUser = Omit<User, 'Id'> & {
	Password: string;
	ConfirmPassword: string;
};