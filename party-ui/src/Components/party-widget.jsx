import {useEffect, useRef, useState} from "react";
import PartyEntryMember from "./party-entry-member";
import {Widget, WidgetButtonRow, WidgetFormRow, WidgetHeader, WidgetInputText, WidgetPage, WidgetRow} from "./widget";
import PartyEntryPending from "./party-entry-pending";
import styled from "styled-components";
import {ConfirmWidgetButton, WidgetFlexButton} from "./buttons";
import {ArrowLeftIcon, GearIcon, XIcon} from "./icons";

const WidgetButton = styled.button`
    all: unset;
    display: flex;
    align-content: end;
    align-items: center;
    padding: 0 8px 0 8px;
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
    padding: 8px;
    user-select: none;
`;

const handleCloseWidget = () => {
    clearInterval(window.party_data_interval);
    clearInterval(window.refresh_party_data_interval);
    document.getElementById("party-root").remove();
};

const CloseWidget = () => {

    return <WidgetButton onClick={handleCloseWidget}>
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

const CreatePartyWidgetRow = ({leader}) => {
    const handleCreateGroup = () => {
        window.modApi.createGroup();
    };

    return (
        <WidgetPage visible={!leader}>
            <WidgetRow>You are not in a group</WidgetRow>
            <WidgetButtonRow>
                <WidgetFlexButton onClick={handleCreateGroup}>Create Group</WidgetFlexButton>
            </WidgetButtonRow>
        </WidgetPage>
    );
};

const PartyWidget = () => {

    const [page, setPage] = useState("members");
    const [members, setMembers] = useState([]);
    const [pendingAccept, setPendingAccept] = useState([]);
    const [invited, setInvited] = useState([]);
    const [leader, setLeader] = useState(null);
    const [inviteName, setInviteName] = useState("");

    const [position, setPosition] = useState({ x: 0, y: 0 });
    const [dragging, setDragging] = useState(false);
    const [offset, setOffset] = useState({ x: 0, y: 0 });
    const mouseLeaveTimeout = useRef(null);

    useEffect(() => {
        const centerX = window.innerWidth / 2 - 200; // Adjust for half the widget width
        const centerY = window.innerHeight / 2 - 200; // Adjust for half the widget height
        setPosition({ x: centerX, y: centerY });
    }, []);

    const fetchData = () => {
        const url = window.global_resources["player-party"];

        fetch(url)
            .then(res => {
                return res.json()
            }, error => {})
            .then(resJson => {
                setMembers(resJson.Members);
                setLeader(resJson.Leader);
                setPendingAccept(resJson.PendingAccept);
                setInvited(resJson.Invited);
            }, error => {});
    };

    useEffect(() => fetchData(), []);

    useEffect(() => {
        if (window.party_data_interval) {
            clearInterval(window.party_data_interval);
        }
        window.party_data_interval = setInterval(() => {
            fetchData();
        }, 250);

        return () => clearInterval(window.party_data_interval);
    }, []);

    useEffect(() => {
        if (window.refresh_party_data_interval) {
            clearInterval(window.refresh_party_data_interval);
        }

        window.refresh_party_data_interval = setInterval(() => {
            window.modApi.refreshPlayerPartyData();
        }, 2000);

        return () => clearInterval(window.refresh_party_data_interval);
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
        clearTimeout(mouseLeaveTimeout.current);

        mouseLeaveTimeout.current = setTimeout(() => {
            setDragging(false);
        }, 2000);
    };

    const handleSetRole = (role) => {
        window.modApi.setRole(role);
    };

    const handleLeaveGroup = () => {
        window.modApi.leaveGroup();
        handleCloseWidget();
    };

    const handleDisbandGroup = () => {
        window.modApi.disbandGroup();
        handleCloseWidget();
    };

    const handlePlayerNameChanged = (e) => {
        setInviteName(e.target.value);
    };

    const handleInvite = () => {
        if (inviteName)
        {
            window.modApi.inviteToGroup(inviteName);
            setInviteName("");
        }
    };

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
                <CreatePartyWidgetRow leader={leader} />
                <WidgetPage visible={page === "members"}>
                    <PartyEntryMember item={leader}/>
                    <PartyMembers data={members}/>
                </WidgetPage>
                <WidgetPage visible={page === "pending"}>
                    <WidgetButtonRow>
                        <WidgetFlexButton onClick={() => handleSetRole("cannon")}>Cannon</WidgetFlexButton>
                        &nbsp;
                        <WidgetFlexButton onClick={() => handleSetRole("laser")}>Laser</WidgetFlexButton>
                        &nbsp;
                        <WidgetFlexButton onClick={() => handleSetRole("missile")}>Missile</WidgetFlexButton>
                        &nbsp;
                        <WidgetFlexButton onClick={() => handleSetRole("railgun")}>Railgun</WidgetFlexButton>
                    </WidgetButtonRow>
                    {/*<WidgetFormRow>*/}
                    {/*    <WidgetInputText placeholder="Player name" onChange={handlePlayerNameChanged} />&nbsp;<WidgetFlexButton onClick={handleInvite}>Invite</WidgetFlexButton>*/}
                    {/*</WidgetFormRow>*/}
                    <WidgetFormRow>
                        <ConfirmWidgetButton className="p50" onConfirm={handleLeaveGroup} confirmClassName="p50 danger">Leave Group</ConfirmWidgetButton>
                        &nbsp;
                        <ConfirmWidgetButton onConfirm={handleDisbandGroup} className="p50 danger" confirmClassName="p50 danger">Disband Group</ConfirmWidgetButton>
                    </WidgetFormRow>
                    <PendingPlayers type={"invited"} data={invited}/>
                    <PendingPlayers type={"pending-accept"} data={pendingAccept}/>
                </WidgetPage>
            </Widget>
        </Container>
    );
};

export default PartyWidget;