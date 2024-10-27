import {useEffect, useState} from "react";
import PartyEntryMember from "./party-entry-member";
import {Widget, WidgetButtonRow, WidgetHeader, WidgetPage} from "./widget";
import PartyEntryPending from "./party-entry-pending";
import styled from "styled-components";
import {WidgetFlexButton} from "./buttons";
import {ArrowLeftIcon, GearIcon, XIcon} from "./icons";

const WidgetButton = styled.button`
    all: unset;
    display: flex;
    align-content: end;
    justify-content: end;
    cursor: pointer;

    &.tilt {
        animation: tilt 1.5s ease-in-out infinite;
    }

    @keyframes tilt {
        0%, 100% {
            transform: rotate(0deg); /* Start and end in the normal position */
        }
        25% {
            transform: rotate(30deg);
        }
        75% {
            transform: rotate(-30deg);
            color: rgb(250, 70, 70);
        }
    }
`;

const WidgetTitle = styled.div`
    justify-content: center;
    align-content: center;
    flex-grow: 1;
    cursor: move;
    margin: 0 30px;
    user-select: none;
`;

const CloseWidget = () => {
    return <WidgetButton>
        <XIcon size={18}/>
    </WidgetButton>;
}

const SettingsButton = ({onClick, visible, count}) => {
    if (!visible) {
        return null;
    }

    return (
        <WidgetButton className={count > 0 ? "tilt" : ""} onClick={onClick}>
            <GearIcon size={18}/>
        </WidgetButton>
    )
}

const BackToMembersButton = ({onClick, visible}) => {
    if (!visible) {
        return null;
    }

    return (
        <WidgetButton onClick={onClick}>
            <ArrowLeftIcon size={18}/>
        </WidgetButton>
    )
}

export const Container = styled.div`
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    z-index: 99999999 !important;
`;

const PartyWidget = () => {

    const [page, setPage] = useState("members");
    const [members, setMembers] = useState([]);
    const [pendingAccept, setPendingAccept] = useState([]);
    const [invited, setInvited] = useState([]);
    const [leader, setLeader] = useState(null);

    const [position, setPosition] = useState({ x: 0, y: 0 });
    const [dragging, setDragging] = useState(false);
    const [offset, setOffset] = useState({ x: 0, y: 0 });

    useEffect(() => {
        const centerX = window.innerWidth / 2 - 200; // Adjust for half the widget width
        const centerY = window.innerHeight / 2 - 200; // Adjust for half the widget height
        setPosition({ x: centerX, y: centerY });
    }, []);

    useEffect(() => {
        const url = window.global_resources["player-party"];

        fetch(url)
            .then(res => {
                return res.json()
            })
            .then(resJson => {
                setMembers(resJson.Members);
                setLeader(resJson.Leader);
                setPendingAccept(resJson.PendingAccept);
                setInvited(resJson.Invited);
            });

    }, []);

    const PartyMembers = ({data}) => {

        return data.map((item, index) =>
            <PartyEntryMember key={index} item={item}/>
        );
    }

    const PendingPlayers = ({data, type}) => {

        return data.map((item, index) =>
            <PartyEntryPending key={index} type={type} item={item}/>
        );
    }

    const handlePendingClick = () => {
        setPage("pending");
    };

    const handleReturnToMembersClick = () => {
        setPage("members");
    };

    const handleMouseDown = (e) => {
        setDragging(true);
        setOffset({
            x: e.clientX - position.x,
            y: e.clientY - position.y,
        });
    };

    const handleMouseUp = () => {
        setDragging(false);
    };

    const handleMouseMove = (e) => {
        if (dragging) {
            setPosition({
                x: e.clientX - offset.x,
                y: e.clientY - offset.y,
            });
        }
    };

    const handleMouseLeave = (e) => {
        setDragging(false);
    }

    return (
        <Container>
            <Widget style={{ left: position.x, top: position.y }} onMouseLeave={handleMouseLeave}>
                <WidgetHeader>
                    <SettingsButton count={invited.length + pendingAccept.length} visible={page === "members"}
                                    onClick={handlePendingClick}/>
                    <BackToMembersButton visible={page !== "members"} onClick={handleReturnToMembersClick}/>
                    <WidgetTitle onMouseDown={handleMouseDown}
                                 onMouseUp={handleMouseUp}
                                 onMouseMove={handleMouseMove}>Group</WidgetTitle>
                    <CloseWidget/>
                </WidgetHeader>
                <WidgetPage visible={page === "members"}>
                    <PartyEntryMember item={leader}/>
                    <PartyMembers data={members}/>
                </WidgetPage>
                <WidgetPage visible={page === "pending"}>
                    <WidgetButtonRow>
                        <WidgetFlexButton>Cannon</WidgetFlexButton>
                        &nbsp;
                        <WidgetFlexButton>Laser</WidgetFlexButton>
                        &nbsp;
                        <WidgetFlexButton>Missile</WidgetFlexButton>
                        &nbsp;
                        <WidgetFlexButton>Railgun</WidgetFlexButton>
                    </WidgetButtonRow>
                    <WidgetButtonRow>
                        <WidgetFlexButton>Leave Group</WidgetFlexButton>
                    </WidgetButtonRow>
                    <WidgetButtonRow>
                        <WidgetFlexButton className="danger">Disband Group</WidgetFlexButton>
                    </WidgetButtonRow>
                    <PendingPlayers type={"invited"} data={invited}/>
                    <PendingPlayers type={"pending-accept"} data={pendingAccept}/>
                </WidgetPage>
            </Widget>
        </Container>
    );
};

export default PartyWidget;