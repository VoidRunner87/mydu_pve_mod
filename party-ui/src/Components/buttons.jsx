import styled from "styled-components";
import {TargetIcon2} from "./icons";
import {useState} from "react";

export const IconButton = styled.button`
    background-color: rgb(250, 212, 122);
    padding: 2px 2px 0;
    font-weight: bold;
    text-transform: uppercase;
    color: black;
    text-align: left;
    border: none;
    cursor: pointer;
`;

export const PrimaryButton = styled.button`
    background-color: rgb(250, 212, 122);
    padding: 8px;
    font-weight: bold;
    text-transform: uppercase;
    color: black;
    text-align: left;
    border: none;
    cursor: pointer;
`;

export const DestructiveButton = styled.button`
    background-color: rgb(250, 80, 80);
    padding: 8px;
    font-weight: bold;
    text-transform: uppercase;
    color: white;
    text-align: left;
    border: none;
    cursor: pointer;
`;

export const WidgetFlexButton = styled.button`
    background-color: transparent;
    color: rgb(180, 221, 235);
    padding: 8px;
    font-weight: bold;
    text-transform: uppercase;
    text-align: left;
    border: 1px solid rgb(182, 223, 237);
    border-radius: 2px;
    cursor: pointer;
    display: flex;
    flex-grow: 1;
    justify-content: center;
    align-items: center;

    &:hover {
        opacity: 0.9;
    }

    &.danger {
        border-color: rgb(250, 80, 80);
        color: rgb(250, 80, 80);
    }

    &.positive {
        border-color: rgb(80, 250, 80);
        color: rgb(80, 250, 80);
    }

    &.p50 {
        width: 50%;
    }
`;

export const TargetButton = ({onClick}) => {
    return (
        <IconButton onClick={onClick}><TargetIcon2/></IconButton>
    );
}

export const ConfirmWidgetButton = ({onConfirm, className, children, confirmClassName, confirmTime = 3}) => {

    const [showConfirm, setShowConfirm] = useState(false);

    const handleClick = () => {
        setShowConfirm(true);

        setTimeout(() => {
            setShowConfirm(false);
        }, confirmTime * 1000);
    };

    if (showConfirm) {
        return (<WidgetFlexButton hidden={!showConfirm} onClick={onConfirm}
                                  className={confirmClassName}>Confirm</WidgetFlexButton>);
    }

    return (
        <WidgetFlexButton hidden={showConfirm} onClick={handleClick}
                          className={className}>{children}</WidgetFlexButton>
    );
}